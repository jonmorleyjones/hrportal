# ESignedPdf - Legal E-Signature Library for .NET

A .NET 8 library for creating legally recognizable e-signed PDF documents with PAdES (PDF Advanced Electronic Signatures) compliance.

## Features

- **PAdES Compliance**: Supports all PAdES signature levels (B, T, LT, LTA)
- **Digital Signatures**: Sign PDFs using X.509 certificates (PFX/PKCS#12, Windows Certificate Store)
- **Timestamping**: Integration with Time Stamping Authorities (TSA) for proof of when signatures were created
- **Certificate Validation**: Embed OCSP responses and CRLs for long-term validation
- **PDF Creation**: Create PDF documents with text, paragraphs, images, and tables
- **Signature Verification**: Verify signed documents and validate certificate chains
- **Visual Signatures**: Customizable signature appearance with images and text

## Installation

Add the project reference to your solution:

```xml
<ProjectReference Include="..\ESignedPdf\ESignedPdf.csproj" />
```

Or register with dependency injection:

```csharp
services.AddESignedPdf();
```

## Quick Start

### Creating and Signing a PDF Document

```csharp
using ESignedPdf;
using ESignedPdf.Models;

// Create the client
var client = new ESignedPdfClient();

// Create a PDF document
var pdf = client.CreateDocument()
    .WithA4PageSize()
    .AddText("Employment Contract", bold: true, fontSize: 18)
    .AddParagraph("This agreement is entered into between...")
    .AddTable(new List<List<string>>
    {
        new() { "Term", "Value" },
        new() { "Start Date", "2024-01-15" },
        new() { "Salary", "$75,000" }
    })
    .Build();

// Sign the document
var certificateSource = CertificateSource.FromPfxFile("certificate.pfx", "password");

var signingOptions = SigningOptions.WithTimestamp(certificateSource)
    with
{
    SignerInfo = new SignerInfo
    {
        Reason = "I approve this contract",
        Location = "London, UK"
    },
    Appearance = SignatureAppearance.BottomRight(1)
};

var result = await client.SignAsync(pdf, signingOptions);

if (result.Success)
{
    await File.WriteAllBytesAsync("signed-contract.pdf", result.SignedDocument!);
}
```

### Signature Levels

The library supports all PAdES signature levels defined by ETSI EN 319 142:

| Level | Description | Use Case |
|-------|-------------|----------|
| **PAdES-B** | Basic signature | Minimum legal requirement |
| **PAdES-T** | With timestamp | Recommended for business use |
| **PAdES-LT** | With validation data | Long-term document archival |
| **PAdES-LTA** | With archive timestamp | Indefinite validity |

```csharp
// PAdES-B (Basic)
var options = SigningOptions.Basic(certificateSource);

// PAdES-T (With Timestamp) - Recommended
var options = SigningOptions.WithTimestamp(certificateSource);

// PAdES-LT (Long-Term Validation)
var options = SigningOptions.LongTerm(certificateSource);

// PAdES-LTA (Long-Term Archival)
var options = SigningOptions.LongTermArchive(certificateSource);
```

### Certificate Sources

```csharp
// From PFX file
var cert = CertificateSource.FromPfxFile("certificate.pfx", "password");

// From PFX data
var cert = CertificateSource.FromPfxData(pfxBytes, "password");

// From Windows Certificate Store
var cert = CertificateSource.FromStore(
    thumbprint: "1234567890ABCDEF",
    storeName: StoreName.My,
    storeLocation: StoreLocation.CurrentUser);

// With certificate chain for LT/LTA
cert = cert.WithAutomaticChain();
// Or
cert = cert.WithCertificateChainFromFolder("./certificates");
```

### Timestamp Configuration

```csharp
// Using built-in TSA services
var tsaConfig = TimestampConfiguration.FreeTsa(); // For testing
var tsaConfig = TimestampConfiguration.DigiCert(); // Production

// Custom TSA
var tsaConfig = new TimestampConfiguration
{
    TsaUrl = "https://your-tsa-server.com/tsr",
    Username = "user",
    Password = "pass",
    HashAlgorithm = HashAlgorithm.Sha256,
    TimeoutSeconds = 30
};
```

### Signature Verification

```csharp
var client = new ESignedPdfClient();

// Verify a signed PDF
var result = client.Verify(signedPdfBytes);

Console.WriteLine($"Valid: {result.IsValid}");
Console.WriteLine($"Modified: {result.DocumentModified}");
Console.WriteLine($"Summary: {result.Summary}");

foreach (var sig in result.Signatures)
{
    Console.WriteLine($"Signer: {sig.SignerName}");
    Console.WriteLine($"Signed at: {sig.SigningTime}");
    Console.WriteLine($"Level: {sig.DetectedLevel}");
    Console.WriteLine($"Valid: {sig.IsValid}");
    Console.WriteLine($"Has Timestamp: {sig.HasTimestamp}");
}
```

### Visual Signature Appearance

```csharp
var appearance = new SignatureAppearance
{
    PageNumber = 1,
    X = 400,
    Y = 50,
    Width = 150,
    Height = 75,
    ShowSignerName = true,
    ShowDate = true,
    ShowReason = true,
    ImagePath = "signature-image.png" // Optional signature graphic
};

// Or use presets
var appearance = SignatureAppearance.BottomRight(1);
var appearance = SignatureAppearance.InvisibleSignature;
```

## Legal Compliance

This library implements signatures that are compliant with:

- **eIDAS Regulation (EU)**: Qualified electronic signatures have equivalent legal effect to handwritten signatures
- **ESIGN Act (US)**: Electronic signatures are legally valid
- **UETA (US)**: Uniform Electronic Transactions Act compliance
- **PAdES Standard**: ETSI EN 319 142 - PDF Advanced Electronic Signatures

### Requirements for Legal Validity

1. **Use a qualified certificate** issued by a trusted Certificate Authority
2. **Include a timestamp** (PAdES-T or higher) for proof of when the signature was created
3. **For long-term validity**, use PAdES-LT or PAdES-LTA to embed validation data

## Dependencies

- **iText7** (AGPL/Commercial) - PDF manipulation and signing
- **BouncyCastle** - Cryptographic operations
- **System.Security.Cryptography.Pkcs** - Certificate handling

## License

The ESignedPdf library uses iText7 which is dual-licensed under AGPL and a commercial license. For commercial use without AGPL requirements, you must obtain a commercial license from iText.

## API Reference

### ESignedPdfClient

Main entry point for the library.

- `Documents` - PDF document service
- `Signatures` - Digital signature service
- `Verification` - Signature verification service
- `Timestamps` - Timestamp service
- `CreateDocument()` - Returns a fluent document builder
- `SignAsync(pdf, options)` - Signs a PDF document
- `Verify(pdf)` - Verifies signatures in a PDF

### SigningOptions

Configuration for signing operations.

- `CertificateSource` - The signing certificate
- `SignatureLevel` - PAdES level (B, T, LT, LTA)
- `SignerInfo` - Reason, location, contact info
- `Appearance` - Visual signature configuration
- `TimestampConfiguration` - TSA settings
- `HashAlgorithm` - SHA-256, SHA-384, or SHA-512
