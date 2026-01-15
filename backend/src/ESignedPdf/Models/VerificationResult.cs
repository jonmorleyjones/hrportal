namespace ESignedPdf.Models;

/// <summary>
/// Result of PDF signature verification.
/// </summary>
public class VerificationResult
{
    /// <summary>
    /// Whether all signatures in the document are valid.
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Whether the document has been modified since it was signed.
    /// </summary>
    public bool DocumentModified { get; set; }

    /// <summary>
    /// List of all signatures found in the document and their verification status.
    /// </summary>
    public List<SignatureVerificationInfo> Signatures { get; set; } = new();

    /// <summary>
    /// Overall verification summary message.
    /// </summary>
    public string? Summary { get; set; }
}

/// <summary>
/// Verification information for a single signature in a PDF document.
/// </summary>
public class SignatureVerificationInfo
{
    /// <summary>
    /// The field name of the signature.
    /// </summary>
    public string? FieldName { get; set; }

    /// <summary>
    /// The name of the signer (from certificate).
    /// </summary>
    public string? SignerName { get; set; }

    /// <summary>
    /// The organization of the signer (from certificate).
    /// </summary>
    public string? Organization { get; set; }

    /// <summary>
    /// Whether this signature is valid (covers the document, certificate is trusted, etc.).
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Whether the signature covers the entire document.
    /// </summary>
    public bool CoversWholeDocument { get; set; }

    /// <summary>
    /// Whether the signing certificate is trusted.
    /// </summary>
    public bool CertificateTrusted { get; set; }

    /// <summary>
    /// Whether the signing certificate has expired.
    /// </summary>
    public bool CertificateExpired { get; set; }

    /// <summary>
    /// Whether the signing certificate has been revoked.
    /// </summary>
    public bool CertificateRevoked { get; set; }

    /// <summary>
    /// The signing date/time (from signature).
    /// </summary>
    public DateTime? SigningTime { get; set; }

    /// <summary>
    /// The timestamp date/time from TSA (if present).
    /// </summary>
    public DateTime? TimestampTime { get; set; }

    /// <summary>
    /// Whether the signature includes a valid timestamp.
    /// </summary>
    public bool HasTimestamp { get; set; }

    /// <summary>
    /// Whether the timestamp is valid.
    /// </summary>
    public bool TimestampValid { get; set; }

    /// <summary>
    /// The detected signature level (PAdES-B, T, LT, or LTA).
    /// </summary>
    public SignatureLevel DetectedLevel { get; set; }

    /// <summary>
    /// The reason for signing (if provided).
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    /// The location where signing took place (if provided).
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    /// Certificate serial number.
    /// </summary>
    public string? CertificateSerialNumber { get; set; }

    /// <summary>
    /// Certificate issuer.
    /// </summary>
    public string? CertificateIssuer { get; set; }

    /// <summary>
    /// Certificate validity start date.
    /// </summary>
    public DateTime? CertificateValidFrom { get; set; }

    /// <summary>
    /// Certificate validity end date.
    /// </summary>
    public DateTime? CertificateValidTo { get; set; }

    /// <summary>
    /// Any warnings or notes about this signature.
    /// </summary>
    public List<string> Warnings { get; set; } = new();

    /// <summary>
    /// Any errors encountered during verification.
    /// </summary>
    public List<string> Errors { get; set; } = new();
}
