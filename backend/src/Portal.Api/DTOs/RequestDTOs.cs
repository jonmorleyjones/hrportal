namespace Portal.Api.DTOs;

// Card display DTO for listing request types
public record RequestTypeCardDto(
    Guid Id,
    string Name,
    string? Description,
    string Icon,
    bool IsActive
);

// Full request type DTO including form JSON
public record RequestTypeDto(
    Guid Id,
    string Name,
    string? Description,
    string Icon,
    int CurrentVersionNumber,
    string FormJson,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

// Response DTO including request type info
public record RequestResponseDto(
    Guid Id,
    Guid RequestTypeId,
    string RequestTypeName,
    Guid UserId,
    string UserName,
    int VersionNumber,
    string ResponseJson,
    bool IsComplete,
    DateTime StartedAt,
    DateTime? CompletedAt
);

// Request DTOs for creating/updating
public record CreateRequestTypeRequest(
    string Name,
    string? Description,
    string? Icon,
    string FormJson
);

public record UpdateRequestTypeRequest(
    string Name,
    string? Description,
    string? Icon,
    string FormJson,
    bool IsActive
);

public record SubmitRequestResponseRequest(
    string ResponseJson,
    bool IsComplete
);
