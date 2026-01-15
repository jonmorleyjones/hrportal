using System.Net.Http.Headers;
using System.Text;
using ESignedPdf.Exceptions;
using ESignedPdf.Interfaces;
using ESignedPdf.Models;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Tsp;
using Org.BouncyCastle.Tsp;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;

namespace ESignedPdf.Services;

/// <summary>
/// Service for obtaining and verifying timestamps from Time Stamping Authorities.
/// </summary>
public class TimestampService : ITimestampService
{
    private static readonly HttpClient _httpClient = new();

    /// <inheritdoc />
    public async Task<byte[]> GetTimestampAsync(byte[] dataHash, TimestampConfiguration configuration)
    {
        try
        {
            // Create timestamp request
            var oid = GetHashAlgorithmOid(configuration.HashAlgorithm);
            var tsqGenerator = new TimeStampRequestGenerator();
            tsqGenerator.SetCertReq(true);

            var nonce = BigInteger.ValueOf(DateTime.UtcNow.Ticks);
            var tsRequest = tsqGenerator.Generate(oid, dataHash, nonce);
            var requestBytes = tsRequest.GetEncoded();

            // Configure HTTP client
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, configuration.TsaUrl);
            httpRequest.Content = new ByteArrayContent(requestBytes);
            httpRequest.Content.Headers.ContentType = new MediaTypeHeaderValue("application/timestamp-query");

            // Add authentication if provided
            if (!string.IsNullOrEmpty(configuration.Username) && !string.IsNullOrEmpty(configuration.Password))
            {
                var credentials = Convert.ToBase64String(
                    Encoding.UTF8.GetBytes($"{configuration.Username}:{configuration.Password}"));
                httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);
            }

            // Send request with timeout
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(configuration.TimeoutSeconds));
            var response = await _httpClient.SendAsync(httpRequest, cts.Token);

            if (!response.IsSuccessStatusCode)
            {
                throw new TimestampException(
                    $"TSA request failed with status {response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
            }

            var responseBytes = await response.Content.ReadAsByteArrayAsync();

            // Parse and validate response
            var tsResponse = new TimeStampResponse(responseBytes);

            if (tsResponse.Status != (int)PkiStatus.Granted &&
                tsResponse.Status != (int)PkiStatus.GrantedWithMods)
            {
                throw new TimestampException(
                    $"TSA returned status {tsResponse.Status}: {tsResponse.GetStatusString()}");
            }

            // Validate the timestamp token
            var tsToken = tsResponse.TimeStampToken;
            if (tsToken == null)
            {
                throw new TimestampException("TSA response does not contain a timestamp token");
            }

            // Verify nonce matches
            var tokenNonce = tsToken.TimeStampInfo.Nonce;
            if (tokenNonce != null && !tokenNonce.Equals(nonce))
            {
                throw new TimestampException("Timestamp response nonce does not match request");
            }

            // Verify hash matches
            var tokenHash = tsToken.TimeStampInfo.GetMessageImprintDigest();
            if (!tokenHash.SequenceEqual(dataHash))
            {
                throw new TimestampException("Timestamp response hash does not match request");
            }

            return tsToken.GetEncoded();
        }
        catch (TimestampException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new TimestampException($"Failed to obtain timestamp from {configuration.TsaUrl}", ex);
        }
    }

    /// <inheritdoc />
    public bool VerifyTimestamp(byte[] timestampToken, byte[] originalDataHash)
    {
        try
        {
            var tsToken = new TimeStampToken(new Org.BouncyCastle.Cms.CmsSignedData(timestampToken));

            // Verify the hash matches
            var tokenHash = tsToken.TimeStampInfo.GetMessageImprintDigest();
            if (!tokenHash.SequenceEqual(originalDataHash))
            {
                return false;
            }

            // Verify the signature on the timestamp
            var signerCerts = tsToken.GetCertificates();
            var signerInfo = tsToken.SignerID;

            var certCollection = signerCerts.EnumerateMatches(signerInfo).Cast<Org.BouncyCastle.X509.X509Certificate>();
            var signerCert = certCollection.FirstOrDefault();

            if (signerCert == null)
            {
                return false;
            }

            tsToken.Validate(signerCert);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <inheritdoc />
    public DateTime GetTimestampDate(byte[] timestampToken)
    {
        try
        {
            var tsToken = new TimeStampToken(new Org.BouncyCastle.Cms.CmsSignedData(timestampToken));
            return tsToken.TimeStampInfo.GenTime;
        }
        catch (Exception ex)
        {
            throw new TimestampException("Failed to extract timestamp date", ex);
        }
    }

    private static string GetHashAlgorithmOid(HashAlgorithm algorithm)
    {
        return algorithm switch
        {
            HashAlgorithm.Sha256 => "2.16.840.1.101.3.4.2.1",
            HashAlgorithm.Sha384 => "2.16.840.1.101.3.4.2.2",
            HashAlgorithm.Sha512 => "2.16.840.1.101.3.4.2.3",
            _ => throw new ArgumentException($"Unsupported hash algorithm: {algorithm}")
        };
    }
}
