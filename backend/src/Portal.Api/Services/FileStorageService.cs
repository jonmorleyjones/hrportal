using Microsoft.Extensions.Options;
using Portal.Api.Models;

namespace Portal.Api.Services;

public class FileStorageOptions
{
    public string BasePath { get; set; } = "./uploads";
    public long MaxFileSizeBytes { get; set; } = 25 * 1024 * 1024; // 25 MB
}

public interface IFileStorageService
{
    Task<UploadedFile> SaveFileAsync(Guid tenantId, Guid userId, string questionName, IFormFile file, CancellationToken ct = default);
    Task<Stream> GetFileStreamAsync(UploadedFile file, CancellationToken ct = default);
    Task DeleteFileAsync(UploadedFile file, CancellationToken ct = default);
    (bool isValid, string? error) ValidateFile(IFormFile file);
}

public class LocalFileStorageService : IFileStorageService
{
    private readonly FileStorageOptions _options;
    private readonly ILogger<LocalFileStorageService> _logger;

    private static readonly HashSet<string> AllowedMimeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        // Documents
        "application/pdf",
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "application/vnd.ms-excel",
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "text/plain",
        // Images
        "image/jpeg",
        "image/png",
        "image/gif"
    };

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".txt",
        ".jpg", ".jpeg", ".png", ".gif"
    };

    private static readonly Dictionary<string, byte[]> MagicBytes = new()
    {
        { ".pdf", new byte[] { 0x25, 0x50, 0x44, 0x46 } },           // %PDF
        { ".png", new byte[] { 0x89, 0x50, 0x4E, 0x47 } },           // PNG signature
        { ".jpg", new byte[] { 0xFF, 0xD8, 0xFF } },                  // JPEG
        { ".jpeg", new byte[] { 0xFF, 0xD8, 0xFF } },
        { ".gif", new byte[] { 0x47, 0x49, 0x46 } },                  // GIF
        { ".doc", new byte[] { 0xD0, 0xCF, 0x11, 0xE0 } },           // OLE compound doc
        { ".xls", new byte[] { 0xD0, 0xCF, 0x11, 0xE0 } },
        { ".docx", new byte[] { 0x50, 0x4B, 0x03, 0x04 } },          // ZIP (Office Open XML)
        { ".xlsx", new byte[] { 0x50, 0x4B, 0x03, 0x04 } }
    };

    public LocalFileStorageService(IOptions<FileStorageOptions> options, ILogger<LocalFileStorageService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public (bool isValid, string? error) ValidateFile(IFormFile file)
    {
        // Check file size
        if (file.Length > _options.MaxFileSizeBytes)
        {
            return (false, $"File size exceeds maximum allowed size of {_options.MaxFileSizeBytes / (1024 * 1024)} MB");
        }

        if (file.Length == 0)
        {
            return (false, "File is empty");
        }

        // Check extension
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(extension) || !AllowedExtensions.Contains(extension))
        {
            return (false, $"File type '{extension}' is not allowed. Allowed types: {string.Join(", ", AllowedExtensions)}");
        }

        // Check MIME type
        if (!AllowedMimeTypes.Contains(file.ContentType))
        {
            return (false, $"Content type '{file.ContentType}' is not allowed");
        }

        return (true, null);
    }

    public async Task<UploadedFile> SaveFileAsync(Guid tenantId, Guid userId, string questionName, IFormFile file, CancellationToken ct = default)
    {
        var validation = ValidateFile(file);
        if (!validation.isValid)
        {
            throw new ArgumentException(validation.error);
        }

        // Validate magic bytes
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!await ValidateMagicBytesAsync(file, extension, ct))
        {
            throw new ArgumentException("File content does not match its extension");
        }

        var storedFileName = Guid.NewGuid().ToString();
        var now = DateTime.UtcNow;
        var relativePath = Path.Combine(
            tenantId.ToString(),
            now.Year.ToString(),
            now.Month.ToString("D2"),
            storedFileName
        );

        var fullPath = Path.Combine(_options.BasePath, relativePath);
        var directory = Path.GetDirectoryName(fullPath)!;

        Directory.CreateDirectory(directory);

        await using var stream = new FileStream(fullPath, FileMode.Create);
        await file.CopyToAsync(stream, ct);

        _logger.LogInformation("Saved file {OriginalFileName} to {StoragePath}", file.FileName, relativePath);

        return new UploadedFile
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UserId = userId,
            QuestionName = questionName,
            OriginalFileName = SanitizeFileName(file.FileName),
            StoredFileName = storedFileName,
            ContentType = file.ContentType,
            FileSizeBytes = file.Length,
            StoragePath = relativePath,
            UploadedAt = now
        };
    }

    public Task<Stream> GetFileStreamAsync(UploadedFile file, CancellationToken ct = default)
    {
        var fullPath = Path.Combine(_options.BasePath, file.StoragePath);

        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException("File not found on storage", file.StoragePath);
        }

        return Task.FromResult<Stream>(new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read));
    }

    public Task DeleteFileAsync(UploadedFile file, CancellationToken ct = default)
    {
        var fullPath = Path.Combine(_options.BasePath, file.StoragePath);

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
            _logger.LogInformation("Deleted file {StoragePath}", file.StoragePath);
        }

        return Task.CompletedTask;
    }

    private async Task<bool> ValidateMagicBytesAsync(IFormFile file, string extension, CancellationToken ct)
    {
        // Text files don't have magic bytes
        if (extension == ".txt")
        {
            return true;
        }

        if (!MagicBytes.TryGetValue(extension, out var expectedBytes))
        {
            return true; // Unknown extension, skip magic byte check
        }

        using var stream = file.OpenReadStream();
        var buffer = new byte[expectedBytes.Length];
        var bytesRead = await stream.ReadAsync(buffer.AsMemory(0, expectedBytes.Length), ct);

        if (bytesRead < expectedBytes.Length)
        {
            return false;
        }

        return buffer.AsSpan(0, expectedBytes.Length).SequenceEqual(expectedBytes);
    }

    private static string SanitizeFileName(string fileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = new string(fileName.Where(c => !invalidChars.Contains(c)).ToArray());

        // Limit length
        if (sanitized.Length > 255)
        {
            var extension = Path.GetExtension(sanitized);
            var nameWithoutExtension = Path.GetFileNameWithoutExtension(sanitized);
            sanitized = nameWithoutExtension[..(255 - extension.Length)] + extension;
        }

        return sanitized;
    }
}
