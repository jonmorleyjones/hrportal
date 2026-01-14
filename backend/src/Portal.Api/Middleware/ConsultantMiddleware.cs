using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Portal.Api.Data;
using Portal.Api.Services;

namespace Portal.Api.Middleware;

public class ConsultantMiddleware
{
    private readonly RequestDelegate _next;

    public ConsultantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, AppDbContext dbContext, ITenantContext tenantContext)
    {
        // Only process if user is authenticated
        if (!context.User.Identity?.IsAuthenticated ?? true)
        {
            await _next(context);
            return;
        }

        // Check if this is a consultant token
        var isConsultantClaim = context.User.FindFirst("is_consultant")?.Value;
        if (isConsultantClaim != "true")
        {
            await _next(context);
            return;
        }

        // Get consultant ID from token
        var consultantIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? context.User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(consultantIdClaim) || !Guid.TryParse(consultantIdClaim, out var consultantId))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { error = "Invalid consultant token" });
            return;
        }

        // Load assigned tenant IDs for this consultant
        var assignedTenantIds = await dbContext.HrConsultantTenantAssignments
            .Where(a => a.HrConsultantId == consultantId && a.IsActive)
            .Select(a => a.TenantId)
            .ToListAsync();

        // Set consultant mode in tenant context
        tenantContext.SetConsultantMode(consultantId, assignedTenantIds);

        // Check for active tenant header (for operations that require a specific tenant)
        if (context.Request.Headers.TryGetValue("X-Active-Tenant-ID", out var activeTenantHeader))
        {
            var activeTenantSlug = activeTenantHeader.ToString();
            if (!string.IsNullOrEmpty(activeTenantSlug))
            {
                // Look up tenant by slug
                var tenant = await dbContext.Tenants
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.Slug == activeTenantSlug && t.IsActive);

                if (tenant != null)
                {
                    // Verify consultant has access to this tenant
                    if (assignedTenantIds.Contains(tenant.Id))
                    {
                        tenantContext.SetActiveTenant(tenant.Id, tenant.Slug);
                    }
                    else
                    {
                        context.Response.StatusCode = 403;
                        await context.Response.WriteAsJsonAsync(new { error = "Consultant does not have access to this tenant" });
                        return;
                    }
                }
            }
        }

        // Also check for active_tenant_id in JWT claims
        var activeTenantIdClaim = context.User.FindFirst("active_tenant_id")?.Value;
        if (!string.IsNullOrEmpty(activeTenantIdClaim) && Guid.TryParse(activeTenantIdClaim, out var activeTenantId))
        {
            if (tenantContext.TenantId == Guid.Empty) // Only if not already set by header
            {
                var tenant = await dbContext.Tenants
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.Id == activeTenantId && t.IsActive);

                if (tenant != null && assignedTenantIds.Contains(tenant.Id))
                {
                    tenantContext.SetActiveTenant(tenant.Id, tenant.Slug);
                }
            }
        }

        await _next(context);
    }
}

public static class ConsultantMiddlewareExtensions
{
    public static IApplicationBuilder UseConsultantMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ConsultantMiddleware>();
    }
}
