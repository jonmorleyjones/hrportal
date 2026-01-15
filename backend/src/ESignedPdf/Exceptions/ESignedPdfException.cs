namespace ESignedPdf.Exceptions;

/// <summary>
/// Base exception for all ESignedPdf library exceptions.
/// </summary>
public class ESignedPdfException : Exception
{
    public ESignedPdfException(string message) : base(message) { }
    public ESignedPdfException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Exception thrown when a signing operation fails.
/// </summary>
public class SigningException : ESignedPdfException
{
    public SigningException(string message) : base(message) { }
    public SigningException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Exception thrown when certificate-related operations fail.
/// </summary>
public class CertificateException : ESignedPdfException
{
    public CertificateException(string message) : base(message) { }
    public CertificateException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Exception thrown when timestamp operations fail.
/// </summary>
public class TimestampException : ESignedPdfException
{
    public TimestampException(string message) : base(message) { }
    public TimestampException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Exception thrown when signature verification fails.
/// </summary>
public class VerificationException : ESignedPdfException
{
    public VerificationException(string message) : base(message) { }
    public VerificationException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Exception thrown when PDF document operations fail.
/// </summary>
public class PdfDocumentException : ESignedPdfException
{
    public PdfDocumentException(string message) : base(message) { }
    public PdfDocumentException(string message, Exception innerException) : base(message, innerException) { }
}
