using ESignedPdf.Models;

namespace ESignedPdf.Interfaces;

/// <summary>
/// Service for verifying digital signatures in PDF documents.
/// </summary>
public interface ISignatureVerificationService
{
    /// <summary>
    /// Verifies all signatures in a PDF document.
    /// </summary>
    /// <param name="pdfDocument">The signed PDF document to verify</param>
    /// <returns>Verification result with details about each signature</returns>
    VerificationResult Verify(byte[] pdfDocument);

    /// <summary>
    /// Verifies all signatures in a PDF file.
    /// </summary>
    /// <param name="filePath">Path to the PDF file to verify</param>
    /// <returns>Verification result</returns>
    VerificationResult VerifyFile(string filePath);

    /// <summary>
    /// Checks if a PDF document contains any signatures.
    /// </summary>
    /// <param name="pdfDocument">The PDF document</param>
    /// <returns>True if the document contains at least one signature</returns>
    bool HasSignatures(byte[] pdfDocument);

    /// <summary>
    /// Gets the number of signatures in a PDF document.
    /// </summary>
    /// <param name="pdfDocument">The PDF document</param>
    /// <returns>Number of signatures</returns>
    int GetSignatureCount(byte[] pdfDocument);

    /// <summary>
    /// Verifies that the document has not been modified since it was signed.
    /// </summary>
    /// <param name="pdfDocument">The signed PDF document</param>
    /// <returns>True if document integrity is intact</returns>
    bool VerifyDocumentIntegrity(byte[] pdfDocument);
}
