using Microsoft.EntityFrameworkCore;
using Portal.Api.Data;
using Portal.Api.Services;

namespace Portal.Api.Middleware;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, AppDbContext dbContext, ITenantContext tenantContext)
    {
        // Skip tenant resolution for certain paths
        var path = context.Request.Path.Value?.ToLower() ?? "";
        if (path.StartsWith("/api/tenants/resolve") ||
            path.StartsWith("/api/auth/hr-login") ||
            path.StartsWith("/swagger") ||
            path.StartsWith("/health"))
        {
            await _next(context);
            return;
        }

        // Try to get tenant from header first
        string? tenantSlug = null;
        if (context.Request.Headers.TryGetValue("X-Tenant-ID", out var headerValue))
        {
            tenantSlug = headerValue.ToString();
        }
        // Fallback to subdomain extraction
        else
        {
            var host = context.Request.Host.Host;
            var parts = host.Split('.');
            if (parts.Length >= 2)
            {
                tenantSlug = parts[0];
            }
        }

        if (string.IsNullOrEmpty(tenantSlug))
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsJsonAsync(new { error = "Tenant not specified" });
            return;
        }

        // Resolve tenant from database
        var tenant = await dbContext.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Slug == tenantSlug && t.IsActive);

        if (tenant == null)
        {
            context.Response.StatusCode = 404;
            await context.Response.WriteAsJsonAsync(new { error = "Tenant not found" });
            return;
        }

        // Set tenant context for this request
        tenantContext.SetTenant(tenant.Id, tenant.Slug);

        await _next(context);
    }
}

public static class TenantMiddlewareExtensions
{
    public static IApplicationBuilder UseTenantMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<TenantMiddleware>();
    }
}
