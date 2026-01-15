using ESignedPdf.Exceptions;
using ESignedPdf.Interfaces;
using ESignedPdf.Models;
using iText.Kernel.Pdf;
using iText.Signatures;
using Org.BouncyCastle.X509;

namespace ESignedPdf.Services;

/// <summary>
/// Service for verifying digital signatures in PDF documents.
/// </summary>
public class SignatureVerificationService : ISignatureVerificationService
{
    /// <inheritdoc />
    public VerificationResult Verify(byte[] pdfDocument)
    {
        try
        {
            using var stream = new MemoryStream(pdfDocument);
            using var reader = new PdfReader(stream);
            using var pdfDoc = new PdfDocument(reader);

            var result = new VerificationResult();
            var signUtil = new SignatureUtil(pdfDoc);
            var signatureNames = signUtil.GetSignatureNames();

            if (signatureNames.Count == 0)
            {
                result.IsValid = true;
                result.Summary = "No signatures found in document";
                return result;
            }

            bool allValid = true;
            bool anyModified = false;

            foreach (var name in signatureNames)
            {
                var sigInfo = VerifySignature(signUtil, name, pdfDoc);
                result.Signatures.Add(sigInfo);

                if (!sigInfo.IsValid)
                {
                    allValid = false;
                }

                if (!sigInfo.CoversWholeDocument)
                {
                    anyModified = true;
                }
            }

            result.IsValid = allValid;
            result.DocumentModified = anyModified;
            result.Summary = GetVerificationSummary(result);

            return result;
        }
        catch (Exception ex)
        {
            throw new VerificationException("Failed to verify PDF signatures", ex);
        }
    }

    /// <inheritdoc />
    public VerificationResult VerifyFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new VerificationException($"File not found: {filePath}");
        }

        var pdfBytes = File.ReadAllBytes(filePath);
        return Verify(pdfBytes);
    }

    /// <inheritdoc />
    public bool HasSignatures(byte[] pdfDocument)
    {
        try
        {
            using var stream = new MemoryStream(pdfDocument);
            using var reader = new PdfReader(stream);
            using var pdfDoc = new PdfDocument(reader);

            var signUtil = new SignatureUtil(pdfDoc);
            return signUtil.GetSignatureNames().Count > 0;
        }
        catch
        {
            return false;
        }
    }

    /// <inheritdoc />
    public int GetSignatureCount(byte[] pdfDocument)
    {
        try
        {
            using var stream = new MemoryStream(pdfDocument);
            using var reader = new PdfReader(stream);
            using var pdfDoc = new PdfDocument(reader);

            var signUtil = new SignatureUtil(pdfDoc);
            return signUtil.GetSignatureNames().Count;
        }
        catch (Exception ex)
        {
            throw new VerificationException("Failed to count signatures", ex);
        }
    }

    /// <inheritdoc />
    public bool VerifyDocumentIntegrity(byte[] pdfDocument)
    {
        try
        {
            var result = Verify(pdfDocument);
            return result.IsValid && !result.DocumentModified;
        }
        catch
        {
            return false;
        }
    }

    private SignatureVerificationInfo VerifySignature(SignatureUtil signUtil, string signatureName, PdfDocument pdfDoc)
    {
        var info = new SignatureVerificationInfo
        {
            FieldName = signatureName
        };

        try
        {
            var pkcs7 = signUtil.ReadSignatureData(signatureName);

            if (pkcs7 == null)
            {
                info.Errors.Add("Could not read signature data");
                info.IsValid = false;
                return info;
            }

            // Basic signature validation
            info.IsValid = pkcs7.VerifySignatureIntegrityAndAuthenticity();

            // Check if signature covers whole document
            info.CoversWholeDocument = signUtil.SignatureCoversWholeDocument(signatureName);
            if (!info.CoversWholeDocument)
            {
                info.Warnings.Add("Document has been modified after this signature was applied");
            }

            // Get signing time
            info.SigningTime = pkcs7.GetSignDate();

            // Get signer certificate info
            var signerCert = pkcs7.GetSigningCertificate();
            if (signerCert != null)
            {
                ExtractCertificateInfo(signerCert, info);
            }

            // Check for timestamp
            var tsToken = pkcs7.GetTimeStampToken();
            if (tsToken != null)
            {
                info.HasTimestamp = true;
                info.TimestampTime = tsToken.TimeStampInfo.GenTime;

                try
                {
                    // Verify timestamp
                    var tsCerts = tsToken.GetCertificates();
                    var tsSignerInfo = tsToken.SignerID;
                    var tsCertCollection = tsCerts.EnumerateMatches(tsSignerInfo).Cast<X509Certificate>();
                    var tsCert = tsCertCollection.FirstOrDefault();
                    if (tsCert != null)
                    {
                        tsToken.Validate(tsCert);
                        info.TimestampValid = true;
                    }
                }
                catch
                {
                    info.TimestampValid = false;
                    info.Warnings.Add("Timestamp verification failed");
                }
            }

            // Get reason and location
            info.Reason = pkcs7.GetReason();
            info.Location = pkcs7.GetLocation();

            // Detect signature level
            info.DetectedLevel = DetectSignatureLevel(pkcs7, info.HasTimestamp);

            // Verify certificate chain
            VerifyCertificateChain(pkcs7, info);
        }
        catch (Exception ex)
        {
            info.IsValid = false;
            info.Errors.Add($"Verification error: {ex.Message}");
        }

        return info;
    }

    private void ExtractCertificateInfo(X509Certificate cert, SignatureVerificationInfo info)
    {
        var subject = cert.SubjectDN.ToString();

        // Extract CN
        info.SignerName = ExtractDnField(subject, "CN");
        info.Organization = ExtractDnField(subject, "O");

        info.CertificateSerialNumber = cert.SerialNumber.ToString(16).ToUpperInvariant();
        info.CertificateIssuer = cert.IssuerDN.ToString();
        info.CertificateValidFrom = cert.NotBefore;
        info.CertificateValidTo = cert.NotAfter;

        // Check certificate validity
        var now = DateTime.UtcNow;
        if (now > cert.NotAfter)
        {
            info.CertificateExpired = true;
            info.Warnings.Add("Signing certificate has expired");
        }
        else if (now < cert.NotBefore)
        {
            info.Warnings.Add("Signing certificate is not yet valid");
        }
    }

    private void VerifyCertificateChain(PdfPKCS7 pkcs7, SignatureVerificationInfo info)
    {
        try
        {
            var certs = pkcs7.GetSignCertificateChain();
            if (certs == null || certs.Length == 0)
            {
                info.Warnings.Add("No certificate chain found");
                return;
            }

            // For now, we just check if the chain is present
            // Full chain validation would require trusted root certificates
            info.CertificateTrusted = certs.Length > 1;

            if (!info.CertificateTrusted)
            {
                info.Warnings.Add("Certificate chain could not be verified to a trusted root");
            }
        }
        catch (Exception ex)
        {
            info.Warnings.Add($"Certificate chain verification failed: {ex.Message}");
        }
    }

    private SignatureLevel DetectSignatureLevel(PdfPKCS7 pkcs7, bool hasTimestamp)
    {
        // Basic detection based on signature attributes
        var hasOcsp = pkcs7.GetOcsp() != null;
        var hasCrls = pkcs7.GetCRLs() != null && pkcs7.GetCRLs().Count > 0;

        if (hasOcsp || hasCrls)
        {
            // Has validation data embedded
            if (hasTimestamp)
            {
                // Could be LT or LTA - need to check for archive timestamp
                // For now, assume LT if we have validation data and timestamp
                return SignatureLevel.PadesLT;
            }
        }

        if (hasTimestamp)
        {
            return SignatureLevel.PadesT;
        }

        return SignatureLevel.PadesB;
    }

    private string? ExtractDnField(string dn, string fieldName)
    {
        var prefix = fieldName + "=";
        var start = dn.IndexOf(prefix, StringComparison.OrdinalIgnoreCase);
        if (start < 0) return null;

        start += prefix.Length;
        var end = dn.IndexOf(',', start);
        if (end < 0) end = dn.Length;

        return dn.Substring(start, end - start).Trim();
    }

    private string GetVerificationSummary(VerificationResult result)
    {
        var count = result.Signatures.Count;
        var validCount = result.Signatures.Count(s => s.IsValid);

        if (count == 0)
        {
            return "Document contains no signatures";
        }

        if (result.IsValid && !result.DocumentModified)
        {
            return $"All {count} signature(s) are valid and the document has not been modified";
        }

        if (result.IsValid && result.DocumentModified)
        {
            return $"All {count} signature(s) are valid, but the document was modified after signing";
        }

        return $"{validCount} of {count} signature(s) are valid";
    }
}
