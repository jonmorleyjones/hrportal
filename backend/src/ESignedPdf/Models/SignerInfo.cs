namespace ESignedPdf.Models;

/// <summary>
/// Information about the signer and the signing context.
/// </summary>
public class SignerInfo
{
    /// <summary>
    /// The reason for signing the document.
    /// Example: "I approve this document", "Contract acceptance", "Document review completed"
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    /// The physical or logical location where the signing took place.
    /// Example: "London, UK", "Remote - Home Office"
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    /// Contact information for the signer.
    /// Example: email address or phone number
    /// </summary>
    public string? ContactInfo { get; set; }

    /// <summary>
    /// The name of the signer. If not provided, will be extracted from the certificate.
    /// </summary>
    public string? SignerName { get; set; }

    /// <summary>
    /// The field name for the signature in the PDF.
    /// If null, a unique name will be generated.
    /// </summary>
    public string? FieldName { get; set; }

    /// <summary>
    /// Whether to certify the document (first signature only).
    /// A certified signature indicates the document originated from the signer
    /// and optionally restricts what changes can be made after signing.
    /// </summary>
    public bool CertifyDocument { get; set; } = false;

    /// <summary>
    /// The level of changes allowed after certification.
    /// Only applicable when CertifyDocument is true.
    /// </summary>
    public CertificationLevel CertificationLevel { get; set; } = CertificationLevel.NoChangesAllowed;
}

/// <summary>
/// Defines what modifications are allowed after a document is certified.
/// </summary>
public enum CertificationLevel
{
    /// <summary>
    /// No changes are allowed to the document after certification.
    /// </summary>
    NoChangesAllowed,

    /// <summary>
    /// Only form filling is allowed after certification.
    /// </summary>
    FormFillingAllowed,

    /// <summary>
    /// Form filling and annotations are allowed after certification.
    /// </summary>
    FormFillingAndAnnotationsAllowed
}
