using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Portal.Api.Data;
using Portal.Api.DTOs;
using Portal.Api.Services;

namespace Portal.Api.Endpoints;

public static class TenantEndpoints
{
    public static void MapTenantEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/tenants").WithTags("Tenants");

        group.MapGet("/resolve", async (string slug, AppDbContext dbContext) =>
        {
            var tenant = await dbContext.Tenants
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Slug == slug && t.IsActive);

            if (tenant == null)
            {
                return Results.NotFound(new { error = "Tenant not found" });
            }

            var settings = string.IsNullOrEmpty(tenant.Settings)
                ? null
                : JsonSerializer.Deserialize<TenantSettings>(tenant.Settings);

            var branding = string.IsNullOrEmpty(tenant.Branding)
                ? null
                : JsonSerializer.Deserialize<TenantBranding>(tenant.Branding);

            return Results.Ok(new TenantDto(
                tenant.Id,
                tenant.Slug,
                tenant.Name,
                tenant.SubscriptionTier,
                settings,
                branding
            ));
        })
        .WithName("ResolveTenant")
        .WithDescription("Validate and resolve tenant by slug")
        .Produces<TenantDto>(200)
        .Produces(404);

        group.MapGet("/current", async (ITenantContext tenantContext, AppDbContext dbContext) =>
        {
            var tenant = await dbContext.Tenants
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == tenantContext.TenantId);

            if (tenant == null)
            {
                return Results.NotFound(new { error = "Tenant not found" });
            }

            var settings = string.IsNullOrEmpty(tenant.Settings)
                ? null
                : JsonSerializer.Deserialize<TenantSettings>(tenant.Settings);

            var branding = string.IsNullOrEmpty(tenant.Branding)
                ? null
                : JsonSerializer.Deserialize<TenantBranding>(tenant.Branding);

            return Results.Ok(new TenantDto(
                tenant.Id,
                tenant.Slug,
                tenant.Name,
                tenant.SubscriptionTier,
                settings,
                branding
            ));
        })
        .RequireAuthorization()
        .WithName("GetCurrentTenant")
        .WithDescription("Get current tenant details")
        .Produces<TenantDto>(200)
        .Produces(404);

        group.MapPut("/settings", async (UpdateTenantSettingsRequest request, ITenantContext tenantContext, AppDbContext dbContext) =>
        {
            var tenant = await dbContext.Tenants
                .FirstOrDefaultAsync(t => t.Id == tenantContext.TenantId);

            if (tenant == null)
            {
                return Results.NotFound(new { error = "Tenant not found" });
            }

            tenant.Settings = JsonSerializer.Serialize(request.Settings);
            await dbContext.SaveChangesAsync();

            return Results.Ok(new { message = "Settings updated successfully" });
        })
        .RequireAuthorization("AdminOnly")
        .WithName("UpdateTenantSettings")
        .WithDescription("Update tenant settings (admin only)")
        .Produces(200)
        .Produces(404);

        group.MapPut("/branding", async (UpdateTenantBrandingRequest request, ITenantContext tenantContext, AppDbContext dbContext) =>
        {
            var tenant = await dbContext.Tenants
                .FirstOrDefaultAsync(t => t.Id == tenantContext.TenantId);

            if (tenant == null)
            {
                return Results.NotFound(new { error = "Tenant not found" });
            }

            tenant.Branding = JsonSerializer.Serialize(request.Branding);
            await dbContext.SaveChangesAsync();

            return Results.Ok(new { message = "Branding updated successfully" });
        })
        .RequireAuthorization("AdminOnly")
        .WithName("UpdateTenantBranding")
        .WithDescription("Update tenant branding (admin only)")
        .Produces(200)
        .Produces(404);
    }
}
