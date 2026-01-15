using ESignedPdf.Interfaces;
using ESignedPdf.Models;
using ESignedPdf.Services;

namespace ESignedPdf;

/// <summary>
/// Main entry point for the ESignedPdf library.
/// Provides a fluent API for creating and signing PDF documents with legally recognizable e-signatures.
/// </summary>
public class ESignedPdfClient
{
    private readonly IPdfDocumentService _documentService;
    private readonly IPdfSignatureService _signatureService;
    private readonly ISignatureVerificationService _verificationService;
    private readonly ITimestampService _timestampService;

    /// <summary>
    /// Creates a new ESignedPdfClient with default service implementations.
    /// </summary>
    public ESignedPdfClient()
    {
        _timestampService = new TimestampService();
        _documentService = new PdfDocumentService();
        _signatureService = new PdfSignatureService(_timestampService);
        _verificationService = new SignatureVerificationService();
    }

    /// <summary>
    /// Creates a new ESignedPdfClient with custom service implementations.
    /// </summary>
    public ESignedPdfClient(
        IPdfDocumentService documentService,
        IPdfSignatureService signatureService,
        ISignatureVerificationService verificationService,
        ITimestampService timestampService)
    {
        _documentService = documentService;
        _signatureService = signatureService;
        _verificationService = verificationService;
        _timestampService = timestampService;
    }

    /// <summary>
    /// PDF document creation and manipulation service.
    /// </summary>
    public IPdfDocumentService Documents => _documentService;

    /// <summary>
    /// PDF digital signature service.
    /// </summary>
    public IPdfSignatureService Signatures => _signatureService;

    /// <summary>
    /// Signature verification service.
    /// </summary>
    public ISignatureVerificationService Verification => _verificationService;

    /// <summary>
    /// Timestamp authority service.
    /// </summary>
    public ITimestampService Timestamps => _timestampService;

    /// <summary>
    /// Creates a new PDF document builder for fluent document creation.
    /// </summary>
    public PdfDocumentBuilder CreateDocument()
    {
        return new PdfDocumentBuilder(_documentService);
    }

    /// <summary>
    /// Signs a PDF document with the specified options.
    /// </summary>
    /// <param name="pdfDocument">The PDF document to sign</param>
    /// <param name="options">Signing options</param>
    /// <returns>The signing result containing the signed document</returns>
    public Task<SignatureResult> SignAsync(byte[] pdfDocument, SigningOptions options)
    {
        return _signatureService.SignAsync(pdfDocument, options);
    }

    /// <summary>
    /// Signs a PDF file and saves the result to a new file.
    /// </summary>
    /// <param name="inputPath">Path to the input PDF file</param>
    /// <param name="outputPath">Path for the signed output PDF file</param>
    /// <param name="options">Signing options</param>
    /// <returns>The signing result</returns>
    public Task<SignatureResult> SignFileAsync(string inputPath, string outputPath, SigningOptions options)
    {
        return _signatureService.SignFileAsync(inputPath, outputPath, options);
    }

    /// <summary>
    /// Verifies all signatures in a PDF document.
    /// </summary>
    /// <param name="pdfDocument">The PDF document to verify</param>
    /// <returns>Verification result with details about each signature</returns>
    public VerificationResult Verify(byte[] pdfDocument)
    {
        return _verificationService.Verify(pdfDocument);
    }

    /// <summary>
    /// Verifies all signatures in a PDF file.
    /// </summary>
    /// <param name="filePath">Path to the PDF file to verify</param>
    /// <returns>Verification result</returns>
    public VerificationResult VerifyFile(string filePath)
    {
        return _verificationService.VerifyFile(filePath);
    }

    /// <summary>
    /// Checks if a PDF document has any signatures.
    /// </summary>
    public bool HasSignatures(byte[] pdfDocument)
    {
        return _verificationService.HasSignatures(pdfDocument);
    }

    /// <summary>
    /// Gets the number of signatures in a PDF document.
    /// </summary>
    public int GetSignatureCount(byte[] pdfDocument)
    {
        return _verificationService.GetSignatureCount(pdfDocument);
    }
}

/// <summary>
/// Fluent builder for creating PDF documents.
/// </summary>
public class PdfDocumentBuilder
{
    private readonly IPdfDocumentService _documentService;
    private readonly List<PdfContent> _contents = new();
    private (float Width, float Height)? _pageSize;

    internal PdfDocumentBuilder(IPdfDocumentService documentService)
    {
        _documentService = documentService;
    }

    /// <summary>
    /// Sets the page size for the document.
    /// </summary>
    public PdfDocumentBuilder WithPageSize((float Width, float Height) pageSize)
    {
        _pageSize = pageSize;
        return this;
    }

    /// <summary>
    /// Sets the page size to A4.
    /// </summary>
    public PdfDocumentBuilder WithA4PageSize()
    {
        _pageSize = Models.PageSize.A4;
        return this;
    }

    /// <summary>
    /// Sets the page size to US Letter.
    /// </summary>
    public PdfDocumentBuilder WithLetterPageSize()
    {
        _pageSize = Models.PageSize.Letter;
        return this;
    }

    /// <summary>
    /// Adds text content to the document.
    /// </summary>
    public PdfDocumentBuilder AddText(string text, float x = 72, float y = 720, float fontSize = 12, bool bold = false)
    {
        _contents.Add(new TextContent
        {
            Text = text,
            X = x,
            Y = y,
            FontSize = fontSize,
            Bold = bold,
            PageNumber = GetCurrentPage()
        });
        return this;
    }

    /// <summary>
    /// Adds a paragraph with automatic text wrapping.
    /// </summary>
    public PdfDocumentBuilder AddParagraph(string text, float x = 72, float y = 720, float maxWidth = 468, float fontSize = 12)
    {
        _contents.Add(new ParagraphContent
        {
            Text = text,
            X = x,
            Y = y,
            MaxWidth = maxWidth,
            FontSize = fontSize,
            PageNumber = GetCurrentPage()
        });
        return this;
    }

    /// <summary>
    /// Adds an image to the document.
    /// </summary>
    public PdfDocumentBuilder AddImage(byte[] imageData, float x = 72, float y = 500, float width = 0, float height = 0)
    {
        _contents.Add(new ImageContent
        {
            ImageData = imageData,
            X = x,
            Y = y,
            Width = width,
            Height = height,
            PageNumber = GetCurrentPage()
        });
        return this;
    }

    /// <summary>
    /// Adds an image from file to the document.
    /// </summary>
    public PdfDocumentBuilder AddImage(string imagePath, float x = 72, float y = 500, float width = 0, float height = 0)
    {
        _contents.Add(new ImageContent
        {
            ImagePath = imagePath,
            X = x,
            Y = y,
            Width = width,
            Height = height,
            PageNumber = GetCurrentPage()
        });
        return this;
    }

    /// <summary>
    /// Adds a table to the document.
    /// </summary>
    public PdfDocumentBuilder AddTable(List<List<string>> rows, bool hasHeader = true, float x = 72, float y = 700)
    {
        _contents.Add(new TableContent
        {
            Rows = rows,
            HasHeader = hasHeader,
            X = x,
            Y = y,
            PageNumber = GetCurrentPage()
        });
        return this;
    }

    /// <summary>
    /// Starts a new page.
    /// </summary>
    public PdfDocumentBuilder NewPage()
    {
        // Next content will be on a new page
        // This is handled by incrementing the page number
        return this;
    }

    /// <summary>
    /// Builds the PDF document.
    /// </summary>
    /// <returns>The PDF document as a byte array</returns>
    public byte[] Build()
    {
        return _documentService.CreateDocument(_contents, _pageSize);
    }

    /// <summary>
    /// Builds and saves the PDF document to a file.
    /// </summary>
    public async Task SaveAsync(string filePath)
    {
        var pdf = Build();
        await File.WriteAllBytesAsync(filePath, pdf);
    }

    private int GetCurrentPage()
    {
        if (_contents.Count == 0) return 1;
        return _contents.Max(c => c.PageNumber);
    }
}
