namespace Portal.Api.DTOs;

public record FileUploadResponseDto(
    Guid Id,
    string OriginalFileName,
    string ContentType,
    long FileSizeBytes,
    string DownloadUrl
);

public record LinkFilesRequest(
    Guid RequestResponseId,
    List<Guid> FileIds
);

public record FileInfoDto(
    Guid Id,
    string OriginalFileName,
    string ContentType,
    long FileSizeBytes,
    string DownloadUrl,
    DateTime UploadedAt
);
