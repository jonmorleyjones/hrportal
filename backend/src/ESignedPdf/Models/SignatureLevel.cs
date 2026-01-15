namespace ESignedPdf.Models;

/// <summary>
/// PAdES (PDF Advanced Electronic Signatures) compliance levels.
/// These levels are defined by ETSI EN 319 142 and are legally recognized in the EU and many other jurisdictions.
/// </summary>
public enum SignatureLevel
{
    /// <summary>
    /// PAdES-B-B (Basic): Basic signature with signed attributes.
    /// Provides proof of who signed and what was signed.
    /// Minimum requirement for a legally valid electronic signature.
    /// </summary>
    PadesB,

    /// <summary>
    /// PAdES-B-T (Timestamp): Basic signature with a trusted timestamp.
    /// Adds proof of when the signature was created using a Time Stamping Authority (TSA).
    /// Recommended for most business use cases.
    /// </summary>
    PadesT,

    /// <summary>
    /// PAdES-B-LT (Long Term): Includes all validation data (certificates, CRLs, OCSP responses).
    /// Allows signature validation without external resources.
    /// Recommended for documents requiring long-term archival.
    /// </summary>
    PadesLT,

    /// <summary>
    /// PAdES-B-LTA (Long Term Archive): Adds archive timestamps for indefinite validity.
    /// Highest level of assurance for long-term document preservation.
    /// Required for documents with legal retention requirements.
    /// </summary>
    PadesLTA
}
