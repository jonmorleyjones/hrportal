using ESignedPdf.Models;
using FluentAssertions;
using Xunit;

namespace ESignedPdf.Tests;

public class ModelsTests
{
    [Fact]
    public void SignatureAppearance_Default_ShouldHaveCorrectValues()
    {
        // Act
        var appearance = SignatureAppearance.Default;

        // Assert
        appearance.PageNumber.Should().Be(1);
        appearance.Invisible.Should().BeFalse();
        appearance.ShowSignerName.Should().BeTrue();
        appearance.ShowDate.Should().BeTrue();
    }

    [Fact]
    public void SignatureAppearance_InvisibleSignature_ShouldBeInvisible()
    {
        // Act
        var appearance = SignatureAppearance.InvisibleSignature;

        // Assert
        appearance.Invisible.Should().BeTrue();
    }

    [Fact]
    public void SignatureAppearance_BottomRight_ShouldHaveCorrectPosition()
    {
        // Act
        var appearance = SignatureAppearance.BottomRight(2);

        // Assert
        appearance.PageNumber.Should().Be(2);
        appearance.X.Should().Be(400);
        appearance.Y.Should().Be(50);
    }

    [Fact]
    public void SignatureAppearance_BottomLeft_ShouldHaveCorrectPosition()
    {
        // Act
        var appearance = SignatureAppearance.BottomLeft(1);

        // Assert
        appearance.PageNumber.Should().Be(1);
        appearance.X.Should().Be(50);
        appearance.Y.Should().Be(50);
    }

    [Fact]
    public void TimestampConfiguration_FreeTsa_ShouldHaveCorrectUrl()
    {
        // Act
        var config = TimestampConfiguration.FreeTsa();

        // Assert
        config.TsaUrl.Should().Be(TimestampConfiguration.WellKnownTsaUrls.FreeTsa);
    }

    [Fact]
    public void TimestampConfiguration_DigiCert_ShouldHaveCorrectUrl()
    {
        // Act
        var config = TimestampConfiguration.DigiCert();

        // Assert
        config.TsaUrl.Should().Be(TimestampConfiguration.WellKnownTsaUrls.DigiCert);
    }

    [Fact]
    public void SignatureResult_Successful_ShouldHaveCorrectProperties()
    {
        // Arrange
        var signedDoc = new byte[] { 0x01, 0x02, 0x03 };

        // Act
        var result = SignatureResult.Successful(signedDoc, SignatureLevel.PadesT, "John Doe");

        // Assert
        result.Success.Should().BeTrue();
        result.SignedDocument.Should().BeSameAs(signedDoc);
        result.SignatureLevel.Should().Be(SignatureLevel.PadesT);
        result.SignerName.Should().Be("John Doe");
        result.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void SignatureResult_Failed_ShouldHaveCorrectProperties()
    {
        // Act
        var result = SignatureResult.Failed("Certificate not found");

        // Assert
        result.Success.Should().BeFalse();
        result.SignedDocument.Should().BeNull();
        result.ErrorMessage.Should().Be("Certificate not found");
    }

    [Fact]
    public void PageSize_Letter_ShouldHaveCorrectDimensions()
    {
        // Assert
        PageSize.Letter.Width.Should().Be(612);
        PageSize.Letter.Height.Should().Be(792);
    }

    [Fact]
    public void PageSize_A4_ShouldHaveCorrectDimensions()
    {
        // Assert
        PageSize.A4.Width.Should().BeApproximately(595.28f, 0.01f);
        PageSize.A4.Height.Should().BeApproximately(841.89f, 0.01f);
    }

    [Fact]
    public void SignerInfo_CertifyDocument_ShouldDefaultToFalse()
    {
        // Act
        var signerInfo = new SignerInfo();

        // Assert
        signerInfo.CertifyDocument.Should().BeFalse();
        signerInfo.CertificationLevel.Should().Be(CertificationLevel.NoChangesAllowed);
    }

    [Fact]
    public void VerificationResult_ShouldInitializeWithEmptyLists()
    {
        // Act
        var result = new VerificationResult();

        // Assert
        result.Signatures.Should().NotBeNull();
        result.Signatures.Should().BeEmpty();
    }

    [Fact]
    public void SignatureVerificationInfo_ShouldInitializeWithEmptyLists()
    {
        // Act
        var info = new SignatureVerificationInfo();

        // Assert
        info.Warnings.Should().NotBeNull();
        info.Warnings.Should().BeEmpty();
        info.Errors.Should().NotBeNull();
        info.Errors.Should().BeEmpty();
    }
}
