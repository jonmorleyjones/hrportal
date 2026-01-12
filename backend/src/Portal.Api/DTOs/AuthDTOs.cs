using Portal.Api.Models;

namespace Portal.Api.DTOs;

public record LoginRequest(string Email, string Password);

public record LoginResponse(string AccessToken, string RefreshToken, UserDto User);

public record RefreshRequest(string RefreshToken);

public record RefreshResponse(string AccessToken, string RefreshToken);

public record ForgotPasswordRequest(string Email);

public record ResetPasswordRequest(string Token, string NewPassword);

public record UserDto(
    Guid Id,
    string Email,
    string Name,
    UserRole Role,
    DateTime? LastLoginAt,
    bool IsActive
);

public record TenantDto(
    Guid Id,
    string Slug,
    string Name,
    string SubscriptionTier,
    TenantSettings? Settings,
    TenantBranding? Branding
);

public record TenantSettings(
    bool EnableNotifications = true,
    string Timezone = "UTC",
    string Language = "en"
);

public record TenantBranding(
    string? LogoUrl = null,
    string PrimaryColor = "#3b82f6",
    string SecondaryColor = "#1e40af"
);

public record ResolveTenantRequest(string Slug);

public record UpdateTenantSettingsRequest(TenantSettings Settings);

public record UpdateTenantBrandingRequest(TenantBranding Branding);
