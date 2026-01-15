namespace ESignedPdf.Models;

/// <summary>
/// Options for configuring a PDF signing operation.
/// </summary>
public class SigningOptions
{
    /// <summary>
    /// The source of the signing certificate.
    /// </summary>
    public required CertificateSource CertificateSource { get; set; }

    /// <summary>
    /// The desired signature level (PAdES-B, T, LT, or LTA).
    /// Default is PAdES-T which includes a timestamp.
    /// </summary>
    public SignatureLevel SignatureLevel { get; set; } = SignatureLevel.PadesT;

    /// <summary>
    /// Information about the signer and signing context.
    /// </summary>
    public SignerInfo SignerInfo { get; set; } = new();

    /// <summary>
    /// Visual appearance configuration for the signature.
    /// </summary>
    public SignatureAppearance Appearance { get; set; } = SignatureAppearance.Default;

    /// <summary>
    /// Timestamp authority configuration.
    /// Required for PAdES-T, PAdES-LT, and PAdES-LTA signatures.
    /// </summary>
    public TimestampConfiguration? TimestampConfiguration { get; set; }

    /// <summary>
    /// Hash algorithm to use for the signature.
    /// </summary>
    public HashAlgorithm HashAlgorithm { get; set; } = HashAlgorithm.Sha256;

    /// <summary>
    /// Whether to embed OCSP responses for certificate validation.
    /// Recommended for PAdES-LT and PAdES-LTA.
    /// </summary>
    public bool EmbedOcspResponses { get; set; } = true;

    /// <summary>
    /// Whether to embed CRL data for certificate validation.
    /// Used when OCSP is not available.
    /// </summary>
    public bool EmbedCrls { get; set; } = true;

    /// <summary>
    /// Creates signing options for a basic PAdES-B signature.
    /// </summary>
    public static SigningOptions Basic(CertificateSource certificateSource) => new()
    {
        CertificateSource = certificateSource,
        SignatureLevel = SignatureLevel.PadesB
    };

    /// <summary>
    /// Creates signing options for a PAdES-T signature with timestamp.
    /// </summary>
    public static SigningOptions WithTimestamp(
        CertificateSource certificateSource,
        TimestampConfiguration? tsaConfig = null) => new()
    {
        CertificateSource = certificateSource,
        SignatureLevel = SignatureLevel.PadesT,
        TimestampConfiguration = tsaConfig ?? TimestampConfiguration.FreeTsa()
    };

    /// <summary>
    /// Creates signing options for a PAdES-LT signature (long-term validation).
    /// </summary>
    public static SigningOptions LongTerm(
        CertificateSource certificateSource,
        TimestampConfiguration? tsaConfig = null) => new()
    {
        CertificateSource = certificateSource,
        SignatureLevel = SignatureLevel.PadesLT,
        TimestampConfiguration = tsaConfig ?? TimestampConfiguration.FreeTsa(),
        EmbedOcspResponses = true,
        EmbedCrls = true
    };

    /// <summary>
    /// Creates signing options for a PAdES-LTA signature (long-term archival).
    /// </summary>
    public static SigningOptions LongTermArchive(
        CertificateSource certificateSource,
        TimestampConfiguration? tsaConfig = null) => new()
    {
        CertificateSource = certificateSource,
        SignatureLevel = SignatureLevel.PadesLTA,
        TimestampConfiguration = tsaConfig ?? TimestampConfiguration.FreeTsa(),
        EmbedOcspResponses = true,
        EmbedCrls = true
    };
}
