using ESignedPdf.Models;

namespace ESignedPdf.Interfaces;

/// <summary>
/// Service for digitally signing PDF documents with legally recognizable e-signatures.
/// </summary>
public interface IPdfSignatureService
{
    /// <summary>
    /// Signs a PDF document with a digital signature.
    /// </summary>
    /// <param name="pdfDocument">The PDF document to sign</param>
    /// <param name="options">Signing options including certificate and appearance</param>
    /// <returns>Result containing the signed document</returns>
    Task<SignatureResult> SignAsync(byte[] pdfDocument, SigningOptions options);

    /// <summary>
    /// Signs a PDF document from a file path.
    /// </summary>
    /// <param name="inputPath">Path to the input PDF file</param>
    /// <param name="outputPath">Path for the signed output PDF file</param>
    /// <param name="options">Signing options</param>
    /// <returns>Result of the signing operation</returns>
    Task<SignatureResult> SignFileAsync(string inputPath, string outputPath, SigningOptions options);

    /// <summary>
    /// Signs a PDF document synchronously.
    /// </summary>
    /// <param name="pdfDocument">The PDF document to sign</param>
    /// <param name="options">Signing options</param>
    /// <returns>Result containing the signed document</returns>
    SignatureResult Sign(byte[] pdfDocument, SigningOptions options);

    /// <summary>
    /// Adds an additional signature to an already signed PDF document.
    /// </summary>
    /// <param name="signedPdfDocument">The already signed PDF document</param>
    /// <param name="options">Signing options for the new signature</param>
    /// <returns>Result containing the document with the additional signature</returns>
    Task<SignatureResult> AddSignatureAsync(byte[] signedPdfDocument, SigningOptions options);

    /// <summary>
    /// Creates a document hash for external signing (e.g., with a hardware token or remote signing service).
    /// </summary>
    /// <param name="pdfDocument">The PDF document to prepare for signing</param>
    /// <param name="options">Signing options (appearance and signer info)</param>
    /// <returns>The hash to be signed externally</returns>
    byte[] PrepareForExternalSigning(byte[] pdfDocument, SigningOptions options);

    /// <summary>
    /// Completes a signature using an externally signed hash.
    /// </summary>
    /// <param name="preparedDocument">The prepared document from PrepareForExternalSigning</param>
    /// <param name="signature">The external signature (PKCS#7/CMS)</param>
    /// <returns>The fully signed document</returns>
    byte[] CompleteExternalSignature(byte[] preparedDocument, byte[] signature);
}
