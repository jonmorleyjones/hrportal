namespace ESignedPdf.Models;

/// <summary>
/// Configuration for connecting to a Time Stamping Authority (TSA).
/// A TSA provides cryptographic proof of when a signature was created.
/// </summary>
public class TimestampConfiguration
{
    /// <summary>
    /// The URL of the Time Stamping Authority service.
    /// </summary>
    public required string TsaUrl { get; set; }

    /// <summary>
    /// Username for TSA authentication (if required).
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Password for TSA authentication (if required).
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// The hash algorithm to use for timestamp requests.
    /// Default is SHA-256 which is recommended for most use cases.
    /// </summary>
    public HashAlgorithm HashAlgorithm { get; set; } = HashAlgorithm.Sha256;

    /// <summary>
    /// Timeout in seconds for TSA requests.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Optional client certificate for TSA authentication (as PFX/PKCS#12 data).
    /// </summary>
    public byte[]? ClientCertificate { get; set; }

    /// <summary>
    /// Password for the client certificate.
    /// </summary>
    public string? ClientCertificatePassword { get; set; }

    /// <summary>
    /// Well-known free TSA services that can be used for testing or non-critical applications.
    /// For production use, consider using a commercial TSA service.
    /// </summary>
    public static class WellKnownTsaUrls
    {
        /// <summary>FreeTSA.org - Free timestamp service</summary>
        public const string FreeTsa = "https://freetsa.org/tsr";

        /// <summary>DigiCert timestamp service</summary>
        public const string DigiCert = "http://timestamp.digicert.com";

        /// <summary>Sectigo (formerly Comodo) timestamp service</summary>
        public const string Sectigo = "http://timestamp.sectigo.com";

        /// <summary>GlobalSign timestamp service</summary>
        public const string GlobalSign = "http://timestamp.globalsign.com/tsa/r6advanced1";

        /// <summary>Apple timestamp service</summary>
        public const string Apple = "http://timestamp.apple.com/ts01";
    }

    /// <summary>
    /// Creates a configuration using the FreeTSA service (suitable for testing).
    /// </summary>
    public static TimestampConfiguration FreeTsa() => new()
    {
        TsaUrl = WellKnownTsaUrls.FreeTsa
    };

    /// <summary>
    /// Creates a configuration using the DigiCert timestamp service.
    /// </summary>
    public static TimestampConfiguration DigiCert() => new()
    {
        TsaUrl = WellKnownTsaUrls.DigiCert
    };
}

/// <summary>
/// Hash algorithms supported for digital signatures and timestamps.
/// </summary>
public enum HashAlgorithm
{
    /// <summary>SHA-256 - Recommended for most use cases</summary>
    Sha256,

    /// <summary>SHA-384 - Higher security level</summary>
    Sha384,

    /// <summary>SHA-512 - Highest security level</summary>
    Sha512
}
