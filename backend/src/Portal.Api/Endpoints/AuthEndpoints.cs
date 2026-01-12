using Portal.Api.DTOs;
using Portal.Api.Services;

namespace Portal.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Authentication");

        group.MapPost("/login", async (LoginRequest request, IAuthService authService, ITenantContext tenantContext) =>
        {
            var result = await authService.LoginAsync(request.Email, request.Password, tenantContext.TenantId);

            if (result == null)
            {
                return Results.Unauthorized();
            }

            return Results.Ok(result);
        })
        .WithName("Login")
        .WithDescription("Authenticate user and receive tokens")
        .Produces<LoginResponse>(200)
        .Produces(401);

        group.MapPost("/refresh", async (RefreshRequest request, IAuthService authService) =>
        {
            var result = await authService.RefreshTokenAsync(request.RefreshToken);

            if (result == null)
            {
                return Results.Unauthorized();
            }

            return Results.Ok(result);
        })
        .WithName("RefreshToken")
        .WithDescription("Refresh access token using refresh token")
        .Produces<RefreshResponse>(200)
        .Produces(401);

        group.MapPost("/logout", async (RefreshRequest request, IAuthService authService) =>
        {
            await authService.LogoutAsync(request.RefreshToken);
            return Results.Ok(new { message = "Logged out successfully" });
        })
        .WithName("Logout")
        .WithDescription("Revoke refresh token")
        .Produces(200);

        group.MapPost("/forgot-password", (ForgotPasswordRequest request) =>
        {
            // TODO: Implement email sending
            return Results.Ok(new { message = "If the email exists, a reset link has been sent" });
        })
        .WithName("ForgotPassword")
        .WithDescription("Request password reset email")
        .Produces(200);

        group.MapPost("/reset-password", (ResetPasswordRequest request) =>
        {
            // TODO: Implement password reset
            return Results.Ok(new { message = "Password reset successfully" });
        })
        .WithName("ResetPassword")
        .WithDescription("Reset password with token")
        .Produces(200)
        .Produces(400);
    }
}
