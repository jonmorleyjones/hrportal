using ESignedPdf.Models;
using FluentAssertions;
using Xunit;

namespace ESignedPdf.Tests;

public class ESignedPdfClientTests
{
    private readonly ESignedPdfClient _client;

    public ESignedPdfClientTests()
    {
        _client = new ESignedPdfClient();
    }

    [Fact]
    public void CreateDocument_ShouldReturnBuilder()
    {
        // Act
        var builder = _client.CreateDocument();

        // Assert
        builder.Should().NotBeNull();
    }

    [Fact]
    public void DocumentBuilder_ShouldCreateValidPdf()
    {
        // Act
        var pdf = _client.CreateDocument()
            .WithLetterPageSize()
            .AddText("Title", bold: true, fontSize: 18)
            .AddParagraph("This is the document content.")
            .Build();

        // Assert
        pdf.Should().NotBeNullOrEmpty();
        var header = System.Text.Encoding.ASCII.GetString(pdf, 0, 4);
        header.Should().Be("%PDF");
    }

    [Fact]
    public void HasSignatures_WithUnsignedPdf_ShouldReturnFalse()
    {
        // Arrange
        var pdf = _client.CreateDocument()
            .AddText("Test document")
            .Build();

        // Act
        var hasSignatures = _client.HasSignatures(pdf);

        // Assert
        hasSignatures.Should().BeFalse();
    }

    [Fact]
    public void GetSignatureCount_WithUnsignedPdf_ShouldReturnZero()
    {
        // Arrange
        var pdf = _client.CreateDocument()
            .AddText("Test document")
            .Build();

        // Act
        var count = _client.GetSignatureCount(pdf);

        // Assert
        count.Should().Be(0);
    }

    [Fact]
    public void Verify_WithUnsignedPdf_ShouldReturnValid()
    {
        // Arrange
        var pdf = _client.CreateDocument()
            .AddText("Test document")
            .Build();

        // Act
        var result = _client.Verify(pdf);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Signatures.Should().BeEmpty();
    }

    [Fact]
    public void DocumentBuilder_WithA4PageSize_ShouldWork()
    {
        // Act
        var pdf = _client.CreateDocument()
            .WithA4PageSize()
            .AddText("A4 Document")
            .Build();

        // Assert
        pdf.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void DocumentBuilder_WithTable_ShouldWork()
    {
        // Arrange
        var tableData = new List<List<string>>
        {
            new() { "Column 1", "Column 2" },
            new() { "Value 1", "Value 2" }
        };

        // Act
        var pdf = _client.CreateDocument()
            .AddTable(tableData)
            .Build();

        // Assert
        pdf.Should().NotBeNullOrEmpty();
    }
}
