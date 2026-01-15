using ESignedPdf.Models;
using ESignedPdf.Services;
using FluentAssertions;
using Xunit;

namespace ESignedPdf.Tests;

public class PdfDocumentServiceTests
{
    private readonly PdfDocumentService _service;

    public PdfDocumentServiceTests()
    {
        _service = new PdfDocumentService();
    }

    [Fact]
    public void CreateSimpleDocument_WithText_ShouldCreateValidPdf()
    {
        // Arrange
        var text = "This is a test document for e-signature.";

        // Act
        var result = _service.CreateSimpleDocument(text, "Test Document");

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Length.Should().BeGreaterThan(0);

        // PDF files start with %PDF
        var header = System.Text.Encoding.ASCII.GetString(result, 0, 4);
        header.Should().Be("%PDF");
    }

    [Fact]
    public void CreateSimpleDocument_WithTextOnly_ShouldCreateValidPdf()
    {
        // Arrange
        var text = "Simple text content without a title.";

        // Act
        var result = _service.CreateSimpleDocument(text);

        // Assert
        result.Should().NotBeNullOrEmpty();
        var header = System.Text.Encoding.ASCII.GetString(result, 0, 4);
        header.Should().Be("%PDF");
    }

    [Fact]
    public void CreateDocument_WithTextContent_ShouldCreateValidPdf()
    {
        // Arrange
        var contents = new List<PdfContent>
        {
            new TextContent
            {
                Text = "Hello World",
                X = 100,
                Y = 700,
                FontSize = 14,
                Bold = true
            }
        };

        // Act
        var result = _service.CreateDocument(contents);

        // Assert
        result.Should().NotBeNullOrEmpty();
        _service.GetPageCount(result).Should().Be(1);
    }

    [Fact]
    public void CreateDocument_WithParagraphContent_ShouldCreateValidPdf()
    {
        // Arrange
        var contents = new List<PdfContent>
        {
            new ParagraphContent
            {
                Text = "This is a longer paragraph that should wrap automatically within the specified width. " +
                       "It contains multiple sentences to test the wrapping behavior of the PDF generator.",
                X = 72,
                Y = 720,
                MaxWidth = 468,
                FontSize = 12
            }
        };

        // Act
        var result = _service.CreateDocument(contents);

        // Assert
        result.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void CreateDocument_WithTableContent_ShouldCreateValidPdf()
    {
        // Arrange
        var tableData = new List<List<string>>
        {
            new() { "Name", "Department", "Status" },
            new() { "John Doe", "Engineering", "Active" },
            new() { "Jane Smith", "Marketing", "Active" }
        };

        var contents = new List<PdfContent>
        {
            new TableContent
            {
                Rows = tableData,
                HasHeader = true,
                X = 72,
                Y = 700
            }
        };

        // Act
        var result = _service.CreateDocument(contents);

        // Assert
        result.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GetPageCount_ShouldReturnCorrectCount()
    {
        // Arrange
        var text = "Single page document";
        var pdf = _service.CreateSimpleDocument(text);

        // Act
        var pageCount = _service.GetPageCount(pdf);

        // Assert
        pageCount.Should().Be(1);
    }

    [Fact]
    public void ExtractText_ShouldExtractContent()
    {
        // Arrange
        var originalText = "Extractable text content";
        var pdf = _service.CreateSimpleDocument(originalText);

        // Act
        var extractedText = _service.ExtractText(pdf);

        // Assert
        extractedText.Should().Contain("Extractable text content");
    }

    [Fact]
    public void MergeDocuments_ShouldCombinePages()
    {
        // Arrange
        var pdf1 = _service.CreateSimpleDocument("First document");
        var pdf2 = _service.CreateSimpleDocument("Second document");

        // Act
        var merged = _service.MergeDocuments(new[] { pdf1, pdf2 });

        // Assert
        merged.Should().NotBeNullOrEmpty();
        _service.GetPageCount(merged).Should().Be(2);
    }

    [Fact]
    public void AddPage_ShouldIncrementPageCount()
    {
        // Arrange
        var pdf = _service.CreateSimpleDocument("Original page");
        var newContent = new List<PdfContent>
        {
            new TextContent { Text = "New page content", X = 72, Y = 720 }
        };

        // Act
        var result = _service.AddPage(pdf, newContent);

        // Assert
        _service.GetPageCount(result).Should().Be(2);
    }

    [Fact]
    public void CreateDocument_WithDifferentPageSize_ShouldWork()
    {
        // Arrange
        var contents = new List<PdfContent>
        {
            new TextContent { Text = "A4 Document", X = 72, Y = 800 }
        };

        // Act
        var result = _service.CreateDocument(contents, PageSize.A4);

        // Assert
        result.Should().NotBeNullOrEmpty();
    }
}
