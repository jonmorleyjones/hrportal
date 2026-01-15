using ESignedPdf.Models;

namespace ESignedPdf.Interfaces;

/// <summary>
/// Service for obtaining timestamps from a Time Stamping Authority (TSA).
/// </summary>
public interface ITimestampService
{
    /// <summary>
    /// Requests a timestamp from the configured TSA.
    /// </summary>
    /// <param name="dataHash">The hash of the data to timestamp</param>
    /// <param name="configuration">TSA configuration</param>
    /// <returns>The timestamp token as a byte array</returns>
    Task<byte[]> GetTimestampAsync(byte[] dataHash, TimestampConfiguration configuration);

    /// <summary>
    /// Verifies a timestamp token.
    /// </summary>
    /// <param name="timestampToken">The timestamp token to verify</param>
    /// <param name="originalDataHash">The original data hash that was timestamped</param>
    /// <returns>True if the timestamp is valid</returns>
    bool VerifyTimestamp(byte[] timestampToken, byte[] originalDataHash);

    /// <summary>
    /// Extracts the timestamp date from a timestamp token.
    /// </summary>
    /// <param name="timestampToken">The timestamp token</param>
    /// <returns>The timestamp date/time in UTC</returns>
    DateTime GetTimestampDate(byte[] timestampToken);
}
