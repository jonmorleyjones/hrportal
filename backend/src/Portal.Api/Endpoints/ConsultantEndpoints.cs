using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Portal.Api.Data;
using Portal.Api.DTOs;
using Portal.Api.Models;
using Portal.Api.Services;

namespace Portal.Api.Endpoints;

public static class ConsultantEndpoints
{
    public static void MapConsultantEndpoints(this IEndpointRouteBuilder app)
    {
        // ============================================
        // AUTHENTICATION ENDPOINTS
        // ============================================
        var authGroup = app.MapGroup("/api/consultant/auth").WithTags("Consultant Authentication");

        authGroup.MapPost("/login", async (ConsultantLoginRequest request, IHrConsultantAuthService authService) =>
        {
            var result = await authService.LoginAsync(request.Email, request.Password);

            if (result == null)
            {
                return Results.Unauthorized();
            }

            return Results.Ok(result);
        })
        .WithName("ConsultantLogin")
        .WithDescription("Authenticate consultant and receive tokens")
        .Produces<ConsultantLoginResponse>(200)
        .Produces(401);

        authGroup.MapPost("/refresh", async (RefreshRequest request, IHrConsultantAuthService authService) =>
        {
            var result = await authService.RefreshTokenAsync(request.RefreshToken);

            if (result == null)
            {
                return Results.Unauthorized();
            }

            return Results.Ok(result);
        })
        .WithName("ConsultantRefreshToken")
        .WithDescription("Refresh consultant access token")
        .Produces<ConsultantRefreshResponse>(200)
        .Produces(401);

        authGroup.MapPost("/logout", async (RefreshRequest request, IHrConsultantAuthService authService) =>
        {
            await authService.LogoutAsync(request.RefreshToken);
            return Results.Ok(new { message = "Logged out successfully" });
        })
        .WithName("ConsultantLogout")
        .WithDescription("Revoke consultant refresh token")
        .Produces(200);

        authGroup.MapGet("/me", async (HttpContext httpContext, AppDbContext dbContext, IHrConsultantAuthService authService) =>
        {
            var consultantId = GetConsultantId(httpContext);
            if (consultantId == null)
            {
                return Results.Unauthorized();
            }

            var consultant = await dbContext.HrConsultants
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == consultantId.Value && c.IsActive);

            if (consultant == null)
            {
                return Results.NotFound(new { error = "Consultant not found" });
            }

            var assignedTenants = await authService.GetAssignedTenantsAsync(consultantId.Value);

            return Results.Ok(new
            {
                consultant = new ConsultantDto(
                    consultant.Id,
                    consultant.Email,
                    consultant.Name,
                    consultant.LastLoginAt,
                    consultant.IsActive
                ),
                assignedTenants
            });
        })
        .RequireAuthorization("ConsultantOnly")
        .WithName("GetConsultantProfile")
        .WithDescription("Get current consultant profile with assigned tenants")
        .Produces(200)
        .Produces(401);

        // ============================================
        // TENANT MANAGEMENT ENDPOINTS
        // ============================================
        var tenantsGroup = app.MapGroup("/api/consultant/tenants").WithTags("Consultant Tenants");

        // List assigned tenants with stats
        tenantsGroup.MapGet("", async (HttpContext httpContext, IHrConsultantAuthService authService) =>
        {
            var consultantId = GetConsultantId(httpContext);
            if (consultantId == null)
            {
                return Results.Unauthorized();
            }

            var tenants = await authService.GetAssignedTenantsAsync(consultantId.Value);
            return Results.Ok(tenants);
        })
        .RequireAuthorization("ConsultantOnly")
        .WithName("GetConsultantTenants")
        .WithDescription("Get all tenants assigned to consultant")
        .Produces<IList<TenantSummaryDto>>(200);

        // Get tenant details
        tenantsGroup.MapGet("/{tenantId:guid}", async (Guid tenantId, HttpContext httpContext, AppDbContext dbContext, ITenantContext tenantContext) =>
        {
            if (!await VerifyTenantAccess(httpContext, dbContext, tenantId))
            {
                return Results.Forbid();
            }

            var tenant = await dbContext.Tenants
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == tenantId && t.IsActive);

            if (tenant == null)
            {
                return Results.NotFound(new { error = "Tenant not found" });
            }

            // Get stats
            var userCount = await dbContext.Users
                .IgnoreQueryFilters()
                .CountAsync(u => u.TenantId == tenantId && u.IsActive);

            var activeRequestTypes = await dbContext.RequestTypes
                .IgnoreQueryFilters()
                .CountAsync(r => r.TenantId == tenantId && r.IsActive);

            var totalResponses = await dbContext.RequestResponses
                .IgnoreQueryFilters()
                .Include(r => r.RequestTypeVersion)
                    .ThenInclude(v => v.RequestType)
                .CountAsync(r => r.RequestTypeVersion.RequestType.TenantId == tenantId);

            var pendingResponses = await dbContext.RequestResponses
                .IgnoreQueryFilters()
                .Include(r => r.RequestTypeVersion)
                    .ThenInclude(v => v.RequestType)
                .CountAsync(r => r.RequestTypeVersion.RequestType.TenantId == tenantId && !r.IsComplete);

            // Get permissions for this tenant
            var consultantId = GetConsultantId(httpContext);
            var assignment = await dbContext.HrConsultantTenantAssignments
                .FirstOrDefaultAsync(a => a.HrConsultantId == consultantId && a.TenantId == tenantId && a.IsActive);

            var permissions = new TenantPermissionsDto(
                assignment?.CanManageRequestTypes ?? false,
                assignment?.CanManageSettings ?? false,
                assignment?.CanManageBranding ?? false,
                assignment?.CanViewResponses ?? false
            );

            var settings = !string.IsNullOrEmpty(tenant.Settings)
                ? JsonSerializer.Deserialize<TenantSettings>(tenant.Settings)
                : null;

            var branding = !string.IsNullOrEmpty(tenant.Branding)
                ? JsonSerializer.Deserialize<TenantBranding>(tenant.Branding)
                : null;

            return Results.Ok(new TenantDetailDto(
                tenant.Id,
                tenant.Slug,
                tenant.Name,
                tenant.SubscriptionTier,
                settings,
                branding,
                userCount,
                activeRequestTypes,
                totalResponses,
                pendingResponses,
                tenant.CreatedAt,
                tenant.IsActive,
                permissions
            ));
        })
        .RequireAuthorization("ConsultantOnly")
        .WithName("GetConsultantTenantDetail")
        .WithDescription("Get detailed tenant information")
        .Produces<TenantDetailDto>(200)
        .Produces(403)
        .Produces(404);

        // Update tenant settings
        tenantsGroup.MapPut("/{tenantId:guid}/settings", async (Guid tenantId, UpdateTenantSettingsRequest request, HttpContext httpContext, AppDbContext dbContext) =>
        {
            if (!await VerifyTenantPermission(httpContext, dbContext, tenantId, a => a.CanManageSettings))
            {
                return Results.Forbid();
            }

            var tenant = await dbContext.Tenants.FirstOrDefaultAsync(t => t.Id == tenantId && t.IsActive);
            if (tenant == null)
            {
                return Results.NotFound(new { error = "Tenant not found" });
            }

            tenant.Settings = JsonSerializer.Serialize(request.Settings);
            await dbContext.SaveChangesAsync();

            return Results.Ok(new { message = "Settings updated successfully" });
        })
        .RequireAuthorization("ConsultantOnly")
        .WithName("UpdateConsultantTenantSettings")
        .WithDescription("Update tenant settings")
        .Produces(200)
        .Produces(403)
        .Produces(404);

        // Update tenant branding
        tenantsGroup.MapPut("/{tenantId:guid}/branding", async (Guid tenantId, UpdateTenantBrandingRequest request, HttpContext httpContext, AppDbContext dbContext) =>
        {
            if (!await VerifyTenantPermission(httpContext, dbContext, tenantId, a => a.CanManageBranding))
            {
                return Results.Forbid();
            }

            var tenant = await dbContext.Tenants.FirstOrDefaultAsync(t => t.Id == tenantId && t.IsActive);
            if (tenant == null)
            {
                return Results.NotFound(new { error = "Tenant not found" });
            }

            tenant.Branding = JsonSerializer.Serialize(request.Branding);
            await dbContext.SaveChangesAsync();

            return Results.Ok(new { message = "Branding updated successfully" });
        })
        .RequireAuthorization("ConsultantOnly")
        .WithName("UpdateConsultantTenantBranding")
        .WithDescription("Update tenant branding")
        .Produces(200)
        .Produces(403)
        .Produces(404);

        // ============================================
        // REQUEST TYPE MANAGEMENT ENDPOINTS
        // ============================================
        var requestTypesGroup = app.MapGroup("/api/consultant/tenants/{tenantId:guid}/request-types").WithTags("Consultant Request Types");

        // List request types for a tenant
        requestTypesGroup.MapGet("", async (Guid tenantId, HttpContext httpContext, AppDbContext dbContext) =>
        {
            if (!await VerifyTenantAccess(httpContext, dbContext, tenantId))
            {
                return Results.Forbid();
            }

            var requestTypes = await dbContext.RequestTypes
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Include(r => r.ActiveVersion)
                .Include(r => r.Versions)
                    .ThenInclude(v => v.Responses)
                .Where(r => r.TenantId == tenantId)
                .OrderBy(r => r.Name)
                .Select(r => new ConsultantRequestTypeDto(
                    r.Id,
                    r.Name,
                    r.Description,
                    r.Icon,
                    r.IsActive,
                    r.CurrentVersionNumber,
                    r.Versions.SelectMany(v => v.Responses).Count(),
                    r.Versions.SelectMany(v => v.Responses).Count(resp => resp.IsComplete),
                    r.CreatedAt,
                    r.UpdatedAt
                ))
                .ToListAsync();

            return Results.Ok(requestTypes);
        })
        .RequireAuthorization("ConsultantOnly")
        .WithName("GetConsultantTenantRequestTypes")
        .WithDescription("Get all request types for a tenant")
        .Produces<List<ConsultantRequestTypeDto>>(200)
        .Produces(403);

        // Create request type for a tenant
        requestTypesGroup.MapPost("", async (Guid tenantId, CreateRequestTypeRequest request, HttpContext httpContext, AppDbContext dbContext) =>
        {
            if (!await VerifyTenantPermission(httpContext, dbContext, tenantId, a => a.CanManageRequestTypes))
            {
                return Results.Forbid();
            }

            var requestType = new RequestType
            {
                TenantId = tenantId,
                Name = request.Name,
                Description = request.Description,
                Icon = request.Icon ?? "clipboard-list",
                CurrentVersionNumber = 1,
                IsActive = true
            };

            dbContext.RequestTypes.Add(requestType);
            await dbContext.SaveChangesAsync();

            // Create the first version
            var version = new RequestTypeVersion
            {
                RequestTypeId = requestType.Id,
                VersionNumber = 1,
                FormJson = request.FormJson
            };

            dbContext.RequestTypeVersions.Add(version);
            await dbContext.SaveChangesAsync();

            // Link the active version
            requestType.ActiveVersionId = version.Id;
            await dbContext.SaveChangesAsync();

            return Results.Created($"/api/consultant/tenants/{tenantId}/request-types/{requestType.Id}",
                new ConsultantRequestTypeDto(
                    requestType.Id,
                    requestType.Name,
                    requestType.Description,
                    requestType.Icon,
                    requestType.IsActive,
                    requestType.CurrentVersionNumber,
                    0,
                    0,
                    requestType.CreatedAt,
                    requestType.UpdatedAt
                ));
        })
        .RequireAuthorization("ConsultantOnly")
        .WithName("CreateConsultantTenantRequestType")
        .WithDescription("Create a new request type for a tenant")
        .Produces<ConsultantRequestTypeDto>(201)
        .Produces(403);

        // Get request type details with form JSON
        requestTypesGroup.MapGet("/{requestTypeId:guid}", async (Guid tenantId, Guid requestTypeId, HttpContext httpContext, AppDbContext dbContext) =>
        {
            if (!await VerifyTenantAccess(httpContext, dbContext, tenantId))
            {
                return Results.Forbid();
            }

            var requestType = await dbContext.RequestTypes
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Include(r => r.ActiveVersion)
                .FirstOrDefaultAsync(r => r.Id == requestTypeId && r.TenantId == tenantId);

            if (requestType == null)
            {
                return Results.NotFound(new { error = "Request type not found" });
            }

            return Results.Ok(new RequestTypeDto(
                requestType.Id,
                requestType.Name,
                requestType.Description,
                requestType.Icon,
                requestType.CurrentVersionNumber,
                requestType.ActiveVersion?.FormJson ?? string.Empty,
                requestType.IsActive,
                requestType.CreatedAt,
                requestType.UpdatedAt
            ));
        })
        .RequireAuthorization("ConsultantOnly")
        .WithName("GetConsultantTenantRequestType")
        .WithDescription("Get request type details with form JSON")
        .Produces<RequestTypeDto>(200)
        .Produces(403)
        .Produces(404);

        // Update request type
        requestTypesGroup.MapPut("/{requestTypeId:guid}", async (Guid tenantId, Guid requestTypeId, UpdateRequestTypeRequest request, HttpContext httpContext, AppDbContext dbContext) =>
        {
            if (!await VerifyTenantPermission(httpContext, dbContext, tenantId, a => a.CanManageRequestTypes))
            {
                return Results.Forbid();
            }

            var requestType = await dbContext.RequestTypes
                .IgnoreQueryFilters()
                .Include(r => r.ActiveVersion)
                .FirstOrDefaultAsync(r => r.Id == requestTypeId && r.TenantId == tenantId);

            if (requestType == null)
            {
                return Results.NotFound(new { error = "Request type not found" });
            }

            // Check if formJson changed - if so, create a new version
            if (!string.IsNullOrEmpty(request.FormJson) && requestType.ActiveVersion?.FormJson != request.FormJson)
            {
                var newVersionNumber = requestType.CurrentVersionNumber + 1;
                var newVersion = new RequestTypeVersion
                {
                    RequestTypeId = requestType.Id,
                    VersionNumber = newVersionNumber,
                    FormJson = request.FormJson
                };

                dbContext.RequestTypeVersions.Add(newVersion);
                await dbContext.SaveChangesAsync();

                requestType.CurrentVersionNumber = newVersionNumber;
                requestType.ActiveVersionId = newVersion.Id;
            }

            requestType.Name = request.Name;
            requestType.Description = request.Description;
            requestType.Icon = request.Icon ?? requestType.Icon;
            requestType.UpdatedAt = DateTime.UtcNow;

            await dbContext.SaveChangesAsync();

            return Results.Ok(new { message = "Request type updated successfully" });
        })
        .RequireAuthorization("ConsultantOnly")
        .WithName("UpdateConsultantTenantRequestType")
        .WithDescription("Update a request type")
        .Produces(200)
        .Produces(403)
        .Produces(404);

        // Update request type status (activate/deactivate)
        requestTypesGroup.MapPut("/{requestTypeId:guid}/status", async (Guid tenantId, Guid requestTypeId, UpdateRequestTypeStatusRequest request, HttpContext httpContext, AppDbContext dbContext) =>
        {
            if (!await VerifyTenantPermission(httpContext, dbContext, tenantId, a => a.CanManageRequestTypes))
            {
                return Results.Forbid();
            }

            var requestType = await dbContext.RequestTypes
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(r => r.Id == requestTypeId && r.TenantId == tenantId);

            if (requestType == null)
            {
                return Results.NotFound(new { error = "Request type not found" });
            }

            requestType.IsActive = request.IsActive;
            requestType.UpdatedAt = DateTime.UtcNow;
            await dbContext.SaveChangesAsync();

            return Results.Ok(new { message = $"Request type {(request.IsActive ? "activated" : "deactivated")} successfully" });
        })
        .RequireAuthorization("ConsultantOnly")
        .WithName("UpdateConsultantTenantRequestTypeStatus")
        .WithDescription("Activate or deactivate a request type")
        .Produces(200)
        .Produces(403)
        .Produces(404);

        // Delete request type (soft delete)
        requestTypesGroup.MapDelete("/{requestTypeId:guid}", async (Guid tenantId, Guid requestTypeId, HttpContext httpContext, AppDbContext dbContext) =>
        {
            if (!await VerifyTenantPermission(httpContext, dbContext, tenantId, a => a.CanManageRequestTypes))
            {
                return Results.Forbid();
            }

            var requestType = await dbContext.RequestTypes
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(r => r.Id == requestTypeId && r.TenantId == tenantId);

            if (requestType == null)
            {
                return Results.NotFound(new { error = "Request type not found" });
            }

            requestType.IsActive = false;
            requestType.UpdatedAt = DateTime.UtcNow;
            await dbContext.SaveChangesAsync();

            return Results.Ok(new { message = "Request type deleted" });
        })
        .RequireAuthorization("ConsultantOnly")
        .WithName("DeleteConsultantTenantRequestType")
        .WithDescription("Delete a request type (soft delete)")
        .Produces(200)
        .Produces(403)
        .Produces(404);

        // ============================================
        // RESPONSES ENDPOINTS
        // ============================================
        var responsesGroup = app.MapGroup("/api/consultant/tenants/{tenantId:guid}/responses").WithTags("Consultant Responses");

        // Get all responses for a tenant
        responsesGroup.MapGet("", async (Guid tenantId, HttpContext httpContext, AppDbContext dbContext) =>
        {
            if (!await VerifyTenantPermission(httpContext, dbContext, tenantId, a => a.CanViewResponses))
            {
                return Results.Forbid();
            }

            var responses = await dbContext.RequestResponses
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Include(r => r.User)
                .Include(r => r.RequestTypeVersion)
                    .ThenInclude(v => v.RequestType)
                .Where(r => r.RequestTypeVersion.RequestType.TenantId == tenantId)
                .OrderByDescending(r => r.StartedAt)
                .Select(r => new RequestResponseDto(
                    r.Id,
                    r.RequestTypeVersion.RequestType.Id,
                    r.RequestTypeVersion.RequestType.Name,
                    r.UserId,
                    r.User.Name,
                    r.RequestTypeVersion.VersionNumber,
                    r.ResponseJson,
                    r.IsComplete,
                    r.StartedAt,
                    r.CompletedAt
                ))
                .ToListAsync();

            return Results.Ok(responses);
        })
        .RequireAuthorization("ConsultantOnly")
        .WithName("GetConsultantTenantResponses")
        .WithDescription("Get all responses for a tenant")
        .Produces<List<RequestResponseDto>>(200)
        .Produces(403);

        // ============================================
        // CROSS-TENANT VIEWS
        // ============================================
        var crossTenantGroup = app.MapGroup("/api/consultant/requests").WithTags("Consultant Cross-Tenant");

        // Get all requests across assigned tenants
        crossTenantGroup.MapGet("", async (HttpContext httpContext, AppDbContext dbContext, ITenantContext tenantContext) =>
        {
            var consultantId = GetConsultantId(httpContext);
            if (consultantId == null)
            {
                return Results.Unauthorized();
            }

            var assignedTenantIds = tenantContext.AssignedTenantIds ?? new List<Guid>();

            var responses = await dbContext.RequestResponses
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Include(r => r.User)
                .Include(r => r.RequestTypeVersion)
                    .ThenInclude(v => v.RequestType)
                        .ThenInclude(rt => rt.Tenant)
                .Where(r => assignedTenantIds.Contains(r.RequestTypeVersion.RequestType.TenantId))
                .OrderByDescending(r => r.StartedAt)
                .Take(100) // Limit for performance
                .Select(r => new CrossTenantRequestDto(
                    r.Id,
                    r.RequestTypeVersion.RequestType.TenantId,
                    r.RequestTypeVersion.RequestType.Tenant.Name,
                    r.RequestTypeVersion.RequestType.Tenant.Slug,
                    r.RequestTypeVersion.RequestType.Id,
                    r.RequestTypeVersion.RequestType.Name,
                    r.RequestTypeVersion.RequestType.Icon,
                    r.UserId,
                    r.User.Name,
                    r.User.Email,
                    r.IsComplete,
                    r.StartedAt,
                    r.CompletedAt
                ))
                .ToListAsync();

            return Results.Ok(responses);
        })
        .RequireAuthorization("ConsultantOnly")
        .WithName("GetConsultantCrossTenantRequests")
        .WithDescription("Get all requests across assigned tenants")
        .Produces<List<CrossTenantRequestDto>>(200);

        // Get dashboard stats
        crossTenantGroup.MapGet("/stats", async (HttpContext httpContext, AppDbContext dbContext, ITenantContext tenantContext) =>
        {
            var consultantId = GetConsultantId(httpContext);
            if (consultantId == null)
            {
                return Results.Unauthorized();
            }

            var assignedTenantIds = tenantContext.AssignedTenantIds ?? new List<Guid>();

            var totalTenants = assignedTenantIds.Count;

            var totalUsers = await dbContext.Users
                .IgnoreQueryFilters()
                .CountAsync(u => assignedTenantIds.Contains(u.TenantId) && u.IsActive);

            var totalRequestTypes = await dbContext.RequestTypes
                .IgnoreQueryFilters()
                .CountAsync(r => assignedTenantIds.Contains(r.TenantId) && r.IsActive);

            var totalResponses = await dbContext.RequestResponses
                .IgnoreQueryFilters()
                .Include(r => r.RequestTypeVersion)
                    .ThenInclude(v => v.RequestType)
                .CountAsync(r => assignedTenantIds.Contains(r.RequestTypeVersion.RequestType.TenantId));

            var pendingResponses = await dbContext.RequestResponses
                .IgnoreQueryFilters()
                .Include(r => r.RequestTypeVersion)
                    .ThenInclude(v => v.RequestType)
                .CountAsync(r => assignedTenantIds.Contains(r.RequestTypeVersion.RequestType.TenantId) && !r.IsComplete);

            var oneWeekAgo = DateTime.UtcNow.AddDays(-7);
            var completedThisWeek = await dbContext.RequestResponses
                .IgnoreQueryFilters()
                .Include(r => r.RequestTypeVersion)
                    .ThenInclude(v => v.RequestType)
                .CountAsync(r => assignedTenantIds.Contains(r.RequestTypeVersion.RequestType.TenantId)
                    && r.IsComplete
                    && r.CompletedAt >= oneWeekAgo);

            return Results.Ok(new ConsultantDashboardStatsDto(
                totalTenants,
                totalUsers,
                totalRequestTypes,
                totalResponses,
                pendingResponses,
                completedThisWeek
            ));
        })
        .RequireAuthorization("ConsultantOnly")
        .WithName("GetConsultantDashboardStats")
        .WithDescription("Get dashboard statistics across all assigned tenants")
        .Produces<ConsultantDashboardStatsDto>(200);
    }

    // ============================================
    // HELPER METHODS
    // ============================================
    private static Guid? GetConsultantId(HttpContext httpContext)
    {
        var consultantIdClaim = httpContext.User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(consultantIdClaim) || !Guid.TryParse(consultantIdClaim, out var consultantId))
        {
            return null;
        }
        return consultantId;
    }

    private static async Task<bool> VerifyTenantAccess(HttpContext httpContext, AppDbContext dbContext, Guid tenantId)
    {
        var consultantId = GetConsultantId(httpContext);
        if (consultantId == null)
        {
            return false;
        }

        return await dbContext.HrConsultantTenantAssignments
            .AnyAsync(a => a.HrConsultantId == consultantId && a.TenantId == tenantId && a.IsActive);
    }

    private static async Task<bool> VerifyTenantPermission(HttpContext httpContext, AppDbContext dbContext, Guid tenantId, Func<HrConsultantTenantAssignment, bool> permissionCheck)
    {
        var consultantId = GetConsultantId(httpContext);
        if (consultantId == null)
        {
            return false;
        }

        var assignment = await dbContext.HrConsultantTenantAssignments
            .FirstOrDefaultAsync(a => a.HrConsultantId == consultantId && a.TenantId == tenantId && a.IsActive);

        if (assignment == null)
        {
            return false;
        }

        return permissionCheck(assignment);
    }
}
