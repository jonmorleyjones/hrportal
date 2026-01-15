using ESignedPdf.Models;

namespace ESignedPdf.Interfaces;

/// <summary>
/// Service for creating and manipulating PDF documents.
/// </summary>
public interface IPdfDocumentService
{
    /// <summary>
    /// Creates a new PDF document with the specified content.
    /// </summary>
    /// <param name="contents">List of content items to add to the PDF</param>
    /// <param name="pageSize">Page size (width, height) in points</param>
    /// <returns>The PDF document as a byte array</returns>
    byte[] CreateDocument(IEnumerable<PdfContent> contents, (float Width, float Height)? pageSize = null);

    /// <summary>
    /// Creates a simple PDF document with text content.
    /// </summary>
    /// <param name="text">Text content for the document</param>
    /// <param name="title">Optional document title</param>
    /// <returns>The PDF document as a byte array</returns>
    byte[] CreateSimpleDocument(string text, string? title = null);

    /// <summary>
    /// Adds a page to an existing PDF document.
    /// </summary>
    /// <param name="existingPdf">Existing PDF document</param>
    /// <param name="contents">Content to add to the new page</param>
    /// <returns>The modified PDF document as a byte array</returns>
    byte[] AddPage(byte[] existingPdf, IEnumerable<PdfContent> contents);

    /// <summary>
    /// Merges multiple PDF documents into one.
    /// </summary>
    /// <param name="documents">PDF documents to merge</param>
    /// <returns>The merged PDF document as a byte array</returns>
    byte[] MergeDocuments(IEnumerable<byte[]> documents);

    /// <summary>
    /// Gets the number of pages in a PDF document.
    /// </summary>
    /// <param name="pdfDocument">The PDF document</param>
    /// <returns>Number of pages</returns>
    int GetPageCount(byte[] pdfDocument);

    /// <summary>
    /// Extracts text content from a PDF document.
    /// </summary>
    /// <param name="pdfDocument">The PDF document</param>
    /// <returns>Extracted text content</returns>
    string ExtractText(byte[] pdfDocument);
}
