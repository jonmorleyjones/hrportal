namespace Portal.Api.DTOs;

// Authentication DTOs
public record ConsultantLoginRequest(string Email, string Password);

public record ConsultantLoginResponse(
    string AccessToken,
    string RefreshToken,
    ConsultantDto Consultant,
    IList<TenantSummaryDto> AssignedTenants
);

public record ConsultantRefreshResponse(string AccessToken, string RefreshToken);

public record ConsultantDto(
    Guid Id,
    string Email,
    string Name,
    DateTime? LastLoginAt,
    bool IsActive
);

// Tenant summary for consultant dashboard
public record TenantSummaryDto(
    Guid Id,
    string Slug,
    string Name,
    string SubscriptionTier,
    int UserCount,
    int ActiveRequestTypes,
    int PendingResponses,
    bool CanManageRequestTypes,
    bool CanManageSettings,
    bool CanManageBranding,
    bool CanViewResponses
);

// Detailed tenant info for consultant
public record TenantDetailDto(
    Guid Id,
    string Slug,
    string Name,
    string SubscriptionTier,
    TenantSettings? Settings,
    TenantBranding? Branding,
    int UserCount,
    int ActiveRequestTypes,
    int TotalResponses,
    int PendingResponses,
    DateTime CreatedAt,
    bool IsActive,
    TenantPermissionsDto Permissions
);

public record TenantPermissionsDto(
    bool CanManageRequestTypes,
    bool CanManageSettings,
    bool CanManageBranding,
    bool CanViewResponses
);

// Cross-tenant request view
public record CrossTenantRequestDto(
    Guid Id,
    Guid TenantId,
    string TenantName,
    string TenantSlug,
    Guid RequestTypeId,
    string RequestTypeName,
    string RequestTypeIcon,
    Guid UserId,
    string UserName,
    string UserEmail,
    bool IsComplete,
    DateTime StartedAt,
    DateTime? CompletedAt
);

// Request type management for consultant
public record ConsultantRequestTypeDto(
    Guid Id,
    string Name,
    string? Description,
    string Icon,
    bool IsActive,
    int CurrentVersionNumber,
    int TotalResponses,
    int CompletedResponses,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

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
    string? FormJson
);

public record UpdateRequestTypeStatusRequest(bool IsActive);

// Consultant profile
public record UpdateConsultantProfileRequest(string Name);

// Stats and summary
public record ConsultantDashboardStatsDto(
    int TotalTenants,
    int TotalUsers,
    int TotalRequestTypes,
    int TotalResponses,
    int PendingResponses,
    int CompletedResponsesThisWeek
);
