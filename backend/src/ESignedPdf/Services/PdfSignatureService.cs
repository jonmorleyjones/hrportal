using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using ESignedPdf.Exceptions;
using ESignedPdf.Interfaces;
using ESignedPdf.Models;
using iText.Kernel.Pdf;
using iText.Signatures;
using iText.Kernel.Geom;
using iText.IO.Image;
using iText.Forms;
using iText.Forms.Fields;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using BcX509Certificate = Org.BouncyCastle.X509.X509Certificate;

namespace ESignedPdf.Services;

/// <summary>
/// Implementation of PDF digital signature service with PAdES compliance.
/// </summary>
public class PdfSignatureService : IPdfSignatureService
{
    private readonly ITimestampService _timestampService;

    public PdfSignatureService() : this(new TimestampService())
    {
    }

    public PdfSignatureService(ITimestampService timestampService)
    {
        _timestampService = timestampService;
    }

    /// <inheritdoc />
    public async Task<SignatureResult> SignAsync(byte[] pdfDocument, SigningOptions options)
    {
        try
        {
            ValidateOptions(options);

            using var inputStream = new MemoryStream(pdfDocument);
            using var outputStream = new MemoryStream();

            var result = await SignInternalAsync(inputStream, outputStream, options);
            result.SignedDocument = outputStream.ToArray();

            return result;
        }
        catch (SigningException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return SignatureResult.Failed($"Signing failed: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<SignatureResult> SignFileAsync(string inputPath, string outputPath, SigningOptions options)
    {
        try
        {
            ValidateOptions(options);

            if (!File.Exists(inputPath))
            {
                throw new SigningException($"Input file not found: {inputPath}");
            }

            var pdfBytes = await File.ReadAllBytesAsync(inputPath);
            var result = await SignAsync(pdfBytes, options);

            if (result.Success && result.SignedDocument != null)
            {
                await File.WriteAllBytesAsync(outputPath, result.SignedDocument);
            }

            return result;
        }
        catch (SigningException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return SignatureResult.Failed($"File signing failed: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public SignatureResult Sign(byte[] pdfDocument, SigningOptions options)
    {
        return SignAsync(pdfDocument, options).GetAwaiter().GetResult();
    }

    /// <inheritdoc />
    public async Task<SignatureResult> AddSignatureAsync(byte[] signedPdfDocument, SigningOptions options)
    {
        // Adding additional signature works the same way - iText handles it automatically
        return await SignAsync(signedPdfDocument, options);
    }

    /// <inheritdoc />
    public byte[] PrepareForExternalSigning(byte[] pdfDocument, SigningOptions options)
    {
        // For external signing workflow - returns the hash to be signed externally
        throw new NotImplementedException("External signing workflow is not yet implemented. " +
            "Use SignAsync with a local certificate instead.");
    }

    /// <inheritdoc />
    public byte[] CompleteExternalSignature(byte[] preparedDocument, byte[] signature)
    {
        throw new NotImplementedException("External signing workflow is not yet implemented.");
    }

    private async Task<SignatureResult> SignInternalAsync(
        MemoryStream inputStream,
        MemoryStream outputStream,
        SigningOptions options)
    {
        var certificate = options.CertificateSource.Certificate!;
        var signerName = GetSignerName(certificate, options.SignerInfo);

        // Convert to BouncyCastle certificates
        var bcCertificates = GetBouncyCastleCertificates(options);
        var privateKey = GetPrivateKey(certificate);

        // Create the signer
        using var reader = new PdfReader(inputStream);
        var signer = new PdfSigner(reader, outputStream, new StampingProperties().UseAppendMode());

        // Set signature field name
        var fieldName = options.SignerInfo.FieldName ?? $"Signature_{DateTime.UtcNow.Ticks}";
        signer.SetFieldName(fieldName);

        // Configure signature appearance
        ConfigureSignatureAppearance(signer, options, signerName);

        // Configure certification if requested
        if (options.SignerInfo.CertifyDocument)
        {
            var certLevel = options.SignerInfo.CertificationLevel switch
            {
                CertificationLevel.NoChangesAllowed => PdfSigner.CERTIFIED_NO_CHANGES_ALLOWED,
                CertificationLevel.FormFillingAllowed => PdfSigner.CERTIFIED_FORM_FILLING,
                CertificationLevel.FormFillingAndAnnotationsAllowed => PdfSigner.CERTIFIED_FORM_FILLING_AND_ANNOTATIONS,
                _ => PdfSigner.CERTIFIED_NO_CHANGES_ALLOWED
            };
            signer.SetCertificationLevel(certLevel);
        }

        // Create signature container based on signature level
        IExternalSignature externalSignature = new PrivateKeySignature(
            privateKey,
            GetDigestAlgorithm(options.HashAlgorithm));

        ITSAClient? tsaClient = null;
        if (options.SignatureLevel >= SignatureLevel.PadesT && options.TimestampConfiguration != null)
        {
            tsaClient = new CustomTsaClient(options.TimestampConfiguration);
        }

        IOcspClient? ocspClient = null;
        ICrlClient? crlClient = null;

        if (options.SignatureLevel >= SignatureLevel.PadesLT)
        {
            if (options.EmbedOcspResponses)
            {
                ocspClient = new OcspClientBouncyCastle(null);
            }
            if (options.EmbedCrls)
            {
                crlClient = new CrlClientOnline();
            }
        }

        // Sign the document
        signer.SignDetached(
            externalSignature,
            bcCertificates,
            crlClient != null ? new List<ICrlClient> { crlClient } : null,
            ocspClient,
            tsaClient,
            0,
            PdfSigner.CryptoStandard.CADES);

        // For PAdES-LTA, add document timestamp
        if (options.SignatureLevel == SignatureLevel.PadesLTA && tsaClient != null)
        {
            outputStream.Position = 0;
            var ltaOutput = new MemoryStream();
            using var ltaReader = new PdfReader(outputStream);
            var stamper = new PdfSigner(ltaReader, ltaOutput, new StampingProperties().UseAppendMode());
            stamper.Timestamp(tsaClient, null);

            outputStream.SetLength(0);
            ltaOutput.Position = 0;
            await ltaOutput.CopyToAsync(outputStream);
        }

        return new SignatureResult
        {
            Success = true,
            SignatureLevel = options.SignatureLevel,
            SignerName = signerName,
            SigningTime = DateTime.UtcNow,
            CertificateSerialNumber = certificate.SerialNumber,
            CertificateIssuer = certificate.Issuer,
            SignatureFieldName = fieldName
        };
    }

    private void ConfigureSignatureAppearance(PdfSigner signer, SigningOptions options, string signerName)
    {
        var appearance = options.Appearance;

        if (appearance.Invisible)
        {
            return;
        }

        var signatureAppearance = signer.GetSignatureAppearance();

        // Set position
        var rect = new Rectangle(
            appearance.X,
            appearance.Y,
            appearance.Width,
            appearance.Height);

        signatureAppearance.SetPageRect(rect);
        signatureAppearance.SetPageNumber(appearance.PageNumber);

        // Set metadata
        if (!string.IsNullOrEmpty(options.SignerInfo.Reason))
        {
            signatureAppearance.SetReason(options.SignerInfo.Reason);
        }

        if (!string.IsNullOrEmpty(options.SignerInfo.Location))
        {
            signatureAppearance.SetLocation(options.SignerInfo.Location);
        }

        if (!string.IsNullOrEmpty(options.SignerInfo.ContactInfo))
        {
            signatureAppearance.SetContact(options.SignerInfo.ContactInfo);
        }

        // Set image if provided
        if (appearance.ImageData != null)
        {
            var imageData = ImageDataFactory.Create(appearance.ImageData);
            signatureAppearance.SetSignatureGraphic(imageData);
            signatureAppearance.SetRenderingMode(SignatureFieldAppearance.RenderingMode.GRAPHIC_AND_DESCRIPTION);
        }
        else if (!string.IsNullOrEmpty(appearance.ImagePath))
        {
            var imageData = ImageDataFactory.Create(appearance.ImagePath);
            signatureAppearance.SetSignatureGraphic(imageData);
            signatureAppearance.SetRenderingMode(SignatureFieldAppearance.RenderingMode.GRAPHIC_AND_DESCRIPTION);
        }
    }

    private BcX509Certificate[] GetBouncyCastleCertificates(SigningOptions options)
    {
        var certs = new List<BcX509Certificate>();

        // Add signing certificate
        var signingCert = ConvertToBcCertificate(options.CertificateSource.Certificate!);
        certs.Add(signingCert);

        // Add chain certificates
        if (options.CertificateSource.CertificateChain != null)
        {
            foreach (var cert in options.CertificateSource.CertificateChain)
            {
                certs.Add(ConvertToBcCertificate(cert));
            }
        }

        return certs.ToArray();
    }

    private BcX509Certificate ConvertToBcCertificate(X509Certificate2 certificate)
    {
        var parser = new X509CertificateParser();
        return parser.ReadCertificate(certificate.RawData);
    }

    private AsymmetricKeyParameter GetPrivateKey(X509Certificate2 certificate)
    {
        var rsa = certificate.GetRSAPrivateKey();
        if (rsa != null)
        {
            var parameters = rsa.ExportParameters(true);
            return DotNetUtilities.GetRsaKeyPair(parameters).Private;
        }

        var ecdsa = certificate.GetECDsaPrivateKey();
        if (ecdsa != null)
        {
            var parameters = ecdsa.ExportParameters(true);
            return DotNetUtilities.GetECKeyPair(parameters).Private;
        }

        throw new SigningException("Certificate does not contain a supported private key (RSA or ECDSA)");
    }

    private string GetSignerName(X509Certificate2 certificate, SignerInfo signerInfo)
    {
        if (!string.IsNullOrEmpty(signerInfo.SignerName))
        {
            return signerInfo.SignerName;
        }

        // Extract CN from certificate subject
        var subject = certificate.Subject;
        var cnStart = subject.IndexOf("CN=", StringComparison.OrdinalIgnoreCase);
        if (cnStart >= 0)
        {
            cnStart += 3;
            var cnEnd = subject.IndexOf(',', cnStart);
            if (cnEnd < 0) cnEnd = subject.Length;
            return subject.Substring(cnStart, cnEnd - cnStart).Trim();
        }

        return certificate.Subject;
    }

    private string GetDigestAlgorithm(HashAlgorithm algorithm)
    {
        return algorithm switch
        {
            HashAlgorithm.Sha256 => DigestAlgorithms.SHA256,
            HashAlgorithm.Sha384 => DigestAlgorithms.SHA384,
            HashAlgorithm.Sha512 => DigestAlgorithms.SHA512,
            _ => DigestAlgorithms.SHA256
        };
    }

    private void ValidateOptions(SigningOptions options)
    {
        if (options.CertificateSource?.Certificate == null)
        {
            throw new SigningException("Certificate source must be provided");
        }

        if (!options.CertificateSource.Certificate.HasPrivateKey)
        {
            throw new SigningException("Certificate must have a private key for signing");
        }

        if (options.SignatureLevel >= SignatureLevel.PadesT && options.TimestampConfiguration == null)
        {
            throw new SigningException(
                $"Timestamp configuration is required for {options.SignatureLevel} signature level");
        }
    }

    /// <summary>
    /// Custom TSA client that uses our TimestampConfiguration
    /// </summary>
    private class CustomTsaClient : ITSAClient
    {
        private readonly TimestampConfiguration _config;

        public CustomTsaClient(TimestampConfiguration config)
        {
            _config = config;
        }

        public int GetTokenSizeEstimate() => 4096;

        public IDigest GetMessageDigest()
        {
            return _config.HashAlgorithm switch
            {
                Models.HashAlgorithm.Sha384 => DigestUtilities.GetDigest("SHA-384"),
                Models.HashAlgorithm.Sha512 => DigestUtilities.GetDigest("SHA-512"),
                _ => DigestUtilities.GetDigest("SHA-256")
            };
        }

        public byte[] GetTimeStampToken(byte[] imprint)
        {
            var service = new TimestampService();
            return service.GetTimestampAsync(imprint, _config).GetAwaiter().GetResult();
        }
    }
}
