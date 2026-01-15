namespace ESignedPdf.Models;

/// <summary>
/// Result of a PDF signing operation.
/// </summary>
public class SignatureResult
{
    /// <summary>
    /// Whether the signing operation was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// The signed PDF document as a byte array.
    /// </summary>
    public byte[]? SignedDocument { get; set; }

    /// <summary>
    /// The signature level that was applied.
    /// </summary>
    public SignatureLevel SignatureLevel { get; set; }

    /// <summary>
    /// The date and time when the document was signed.
    /// </summary>
    public DateTime SigningTime { get; set; }

    /// <summary>
    /// The timestamp from the TSA (if PAdES-T or higher was used).
    /// </summary>
    public DateTime? TimestampTime { get; set; }

    /// <summary>
    /// The name of the signer (from certificate).
    /// </summary>
    public string? SignerName { get; set; }

    /// <summary>
    /// The serial number of the signing certificate.
    /// </summary>
    public string? CertificateSerialNumber { get; set; }

    /// <summary>
    /// The certificate issuer.
    /// </summary>
    public string? CertificateIssuer { get; set; }

    /// <summary>
    /// Error message if the operation failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// The field name of the signature in the PDF.
    /// </summary>
    public string? SignatureFieldName { get; set; }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    public static SignatureResult Successful(byte[] signedDocument, SignatureLevel level, string signerName) => new()
    {
        Success = true,
        SignedDocument = signedDocument,
        SignatureLevel = level,
        SignerName = signerName,
        SigningTime = DateTime.UtcNow
    };

    /// <summary>
    /// Creates a failed result.
    /// </summary>
    public static SignatureResult Failed(string errorMessage) => new()
    {
        Success = false,
        ErrorMessage = errorMessage
    };
}
