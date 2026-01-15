using System.Security.Cryptography.X509Certificates;

namespace ESignedPdf.Models;

/// <summary>
/// Represents a source of a signing certificate and its private key.
/// </summary>
public class CertificateSource
{
    /// <summary>
    /// The certificate with private key for signing.
    /// </summary>
    public X509Certificate2? Certificate { get; private set; }

    /// <summary>
    /// The certificate chain (intermediate and root certificates).
    /// </summary>
    public X509Certificate2Collection? CertificateChain { get; private set; }

    /// <summary>
    /// Creates a certificate source from a PFX/PKCS#12 file.
    /// </summary>
    /// <param name="pfxPath">Path to the PFX file</param>
    /// <param name="password">Password for the PFX file</param>
    public static CertificateSource FromPfxFile(string pfxPath, string password)
    {
        var cert = new X509Certificate2(pfxPath, password,
            X509KeyStorageFlags.Exportable | X509KeyStorageFlags.PersistKeySet);

        return new CertificateSource
        {
            Certificate = cert
        };
    }

    /// <summary>
    /// Creates a certificate source from PFX/PKCS#12 data.
    /// </summary>
    /// <param name="pfxData">PFX data as byte array</param>
    /// <param name="password">Password for the PFX data</param>
    public static CertificateSource FromPfxData(byte[] pfxData, string password)
    {
        var cert = new X509Certificate2(pfxData, password,
            X509KeyStorageFlags.Exportable | X509KeyStorageFlags.PersistKeySet);

        return new CertificateSource
        {
            Certificate = cert
        };
    }

    /// <summary>
    /// Creates a certificate source from the Windows certificate store.
    /// </summary>
    /// <param name="thumbprint">Certificate thumbprint to find</param>
    /// <param name="storeName">Certificate store name (default: My/Personal)</param>
    /// <param name="storeLocation">Store location (default: CurrentUser)</param>
    public static CertificateSource FromStore(
        string thumbprint,
        StoreName storeName = StoreName.My,
        StoreLocation storeLocation = StoreLocation.CurrentUser)
    {
        using var store = new X509Store(storeName, storeLocation);
        store.Open(OpenFlags.ReadOnly);

        var certs = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false);
        if (certs.Count == 0)
        {
            throw new InvalidOperationException($"Certificate with thumbprint {thumbprint} not found in store");
        }

        var cert = certs[0];
        if (!cert.HasPrivateKey)
        {
            throw new InvalidOperationException("Certificate does not have a private key");
        }

        return new CertificateSource
        {
            Certificate = cert
        };
    }

    /// <summary>
    /// Creates a certificate source from an existing X509Certificate2 object.
    /// </summary>
    /// <param name="certificate">The certificate with private key</param>
    public static CertificateSource FromCertificate(X509Certificate2 certificate)
    {
        if (!certificate.HasPrivateKey)
        {
            throw new InvalidOperationException("Certificate must have a private key for signing");
        }

        return new CertificateSource
        {
            Certificate = certificate
        };
    }

    /// <summary>
    /// Adds certificate chain for the signing certificate.
    /// This is important for PAdES-LT and PAdES-LTA signatures.
    /// </summary>
    /// <param name="chainCertificates">Collection of intermediate and root certificates</param>
    public CertificateSource WithCertificateChain(X509Certificate2Collection chainCertificates)
    {
        CertificateChain = chainCertificates;
        return this;
    }

    /// <summary>
    /// Adds certificate chain from a folder containing certificate files.
    /// </summary>
    /// <param name="folderPath">Path to folder containing .cer, .crt, or .pem files</param>
    public CertificateSource WithCertificateChainFromFolder(string folderPath)
    {
        var chain = new X509Certificate2Collection();
        var files = Directory.GetFiles(folderPath, "*.*")
            .Where(f => f.EndsWith(".cer", StringComparison.OrdinalIgnoreCase) ||
                       f.EndsWith(".crt", StringComparison.OrdinalIgnoreCase) ||
                       f.EndsWith(".pem", StringComparison.OrdinalIgnoreCase));

        foreach (var file in files)
        {
            chain.Add(new X509Certificate2(file));
        }

        CertificateChain = chain;
        return this;
    }

    /// <summary>
    /// Automatically builds the certificate chain using the system's certificate store.
    /// </summary>
    public CertificateSource WithAutomaticChain()
    {
        if (Certificate == null)
            throw new InvalidOperationException("Certificate must be set before building chain");

        var chain = new X509Chain();
        chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
        chain.Build(Certificate);

        var chainCerts = new X509Certificate2Collection();
        foreach (var element in chain.ChainElements)
        {
            if (element.Certificate.Thumbprint != Certificate.Thumbprint)
            {
                chainCerts.Add(element.Certificate);
            }
        }

        CertificateChain = chainCerts;
        return this;
    }
}
