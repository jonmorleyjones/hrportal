using ESignedPdf.Services;
using FluentAssertions;
using Xunit;

namespace ESignedPdf.Tests;

public class SignatureVerificationServiceTests
{
    private readonly SignatureVerificationService _verificationService;
    private readonly PdfDocumentService _documentService;

    public SignatureVerificationServiceTests()
    {
        _verificationService = new SignatureVerificationService();
        _documentService = new PdfDocumentService();
    }

    [Fact]
    public void HasSignatures_WithUnsignedDocument_ShouldReturnFalse()
    {
        // Arrange
        var pdf = _documentService.CreateSimpleDocument("Unsigned document");

        // Act
        var hasSignatures = _verificationService.HasSignatures(pdf);

        // Assert
        hasSignatures.Should().BeFalse();
    }

    [Fact]
    public void GetSignatureCount_WithUnsignedDocument_ShouldReturnZero()
    {
        // Arrange
        var pdf = _documentService.CreateSimpleDocument("Unsigned document");

        // Act
        var count = _verificationService.GetSignatureCount(pdf);

        // Assert
        count.Should().Be(0);
    }

    [Fact]
    public void Verify_WithUnsignedDocument_ShouldReturnValidWithNoSignatures()
    {
        // Arrange
        var pdf = _documentService.CreateSimpleDocument("Unsigned document");

        // Act
        var result = _verificationService.Verify(pdf);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Signatures.Should().BeEmpty();
        result.Summary.Should().Contain("No signatures");
    }

    [Fact]
    public void VerifyDocumentIntegrity_WithUnsignedDocument_ShouldReturnTrue()
    {
        // Arrange
        var pdf = _documentService.CreateSimpleDocument("Unsigned document");

        // Act
        var isValid = _verificationService.VerifyDocumentIntegrity(pdf);

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void HasSignatures_WithInvalidData_ShouldReturnFalse()
    {
        // Arrange
        var invalidData = new byte[] { 0x00, 0x01, 0x02, 0x03 };

        // Act
        var hasSignatures = _verificationService.HasSignatures(invalidData);

        // Assert
        hasSignatures.Should().BeFalse();
    }
}
