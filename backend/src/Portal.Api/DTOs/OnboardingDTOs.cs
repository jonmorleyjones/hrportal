namespace Portal.Api.DTOs;

public record OnboardingSurveyVersionDto(
    Guid Id,
    int VersionNumber,
    string SurveyJson,
    DateTime CreatedAt
);

public record OnboardingSurveyDto(
    Guid Id,
    string Name,
    int CurrentVersionNumber,
    string SurveyJson,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record OnboardingResponseDto(
    Guid Id,
    Guid UserId,
    string UserName,
    int VersionNumber,
    string ResponseJson,
    bool IsComplete,
    DateTime StartedAt,
    DateTime? CompletedAt
);

public record OnboardingStatusDto(
    bool HasSurvey,
    OnboardingSurveyDto? Survey
);

public record CreateOnboardingSurveyRequest(string Name, string SurveyJson);

public record UpdateOnboardingSurveyRequest(string Name, string SurveyJson, bool IsActive);

public record SubmitOnboardingResponseRequest(string ResponseJson, bool IsComplete);
