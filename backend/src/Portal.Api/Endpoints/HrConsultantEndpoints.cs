using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Portal.Api.Data;
using Portal.Api.DTOs;
using Portal.Api.Models;

namespace Portal.Api.Endpoints;

public static class HrConsultantEndpoints
{
    public static void MapHrConsultantEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/hr")
            .WithTags("HR Consultant")
            .RequireAuthorization();

        group.MapGet("/dashboard/stats", async (
            HttpContext httpContext,
            AppDbContext dbContext) =>
        {
            var consultantId = GetConsultantId(httpContext);
            if (consultantId == null)
            {
                return Results.Unauthorized();
            }

            // Get all tenant IDs the consultant has access to
            var tenantIds = await dbContext.HrConsultantTenantAssignments
                .Where(a => a.HrConsultantId == consultantId && a.IsActive)
                .Select(a => a.TenantId)
                .ToListAsync();

            if (tenantIds.Count == 0)
            {
                return Results.Ok(new HrDashboardStatsResponse(0, 0, 0, 0));
            }

            // Get stats across all assigned tenants
            var totalTenants = tenantIds.Count;

            // Get total responses across all tenants (uses the tenant filter in query)
            var totalResponses = await dbContext.RequestResponses
                .Where(r => tenantIds.Contains(r.RequestTypeVersion!.RequestType!.TenantId))
                .CountAsync();

            // Get pending (incomplete) responses
            var pendingReview = await dbContext.RequestResponses
                .Where(r => tenantIds.Contains(r.RequestTypeVersion!.RequestType!.TenantId) && !r.IsComplete)
                .CountAsync();

            // Calculate completion rate
            var completedResponses = totalResponses - pendingReview;
            var completionRate = totalResponses > 0
                ? Math.Round((decimal)completedResponses / totalResponses * 100, 1)
                : 0;

            return Results.Ok(new HrDashboardStatsResponse(
                totalTenants,
                totalResponses,
                pendingReview,
                completionRate
            ));
        })
        .WithName("GetHrDashboardStats")
        .WithDescription("Get HR consultant dashboard statistics")
        .Produces<HrDashboardStatsResponse>(200);

        group.MapGet("/tenants/{tenantId}/stats", async (
            Guid tenantId,
            HttpContext httpContext,
            AppDbContext dbContext) =>
        {
            var consultantId = GetConsultantId(httpContext);
            if (consultantId == null)
            {
                return Results.Unauthorized();
            }

            // Verify consultant has access to this tenant
            var hasAccess = await dbContext.HrConsultantTenantAssignments
                .AnyAsync(a => a.HrConsultantId == consultantId
                    && a.TenantId == tenantId
                    && a.IsActive);

            if (!hasAccess)
            {
                return Results.Forbid();
            }

            // Get stats for this specific tenant
            var totalUsers = await dbContext.Users
                .Where(u => u.TenantId == tenantId && u.IsActive)
                .CountAsync();

            var totalResponses = await dbContext.RequestResponses
                .Where(r => r.RequestTypeVersion!.RequestType!.TenantId == tenantId)
                .CountAsync();

            var pendingResponses = await dbContext.RequestResponses
                .Where(r => r.RequestTypeVersion!.RequestType!.TenantId == tenantId && !r.IsComplete)
                .CountAsync();

            var requestTypesCount = await dbContext.RequestTypes
                .Where(rt => rt.TenantId == tenantId && rt.IsActive)
                .CountAsync();

            var completedResponses = totalResponses - pendingResponses;
            var completionRate = totalResponses > 0
                ? Math.Round((decimal)completedResponses / totalResponses * 100, 1)
                : 0;

            return Results.Ok(new TenantStatsResponse(
                totalUsers,
                totalResponses,
                pendingResponses,
                requestTypesCount,
                completionRate
            ));
        })
        .WithName("GetTenantStats")
        .WithDescription("Get statistics for a specific tenant")
        .Produces<TenantStatsResponse>(200);

        group.MapGet("/tenants/{tenantId}/responses", async (
            Guid tenantId,
            HttpContext httpContext,
            AppDbContext dbContext,
            int page = 1,
            int pageSize = 20,
            bool? isComplete = null) =>
        {
            var consultantId = GetConsultantId(httpContext);
            if (consultantId == null)
            {
                return Results.Unauthorized();
            }

            // Verify consultant has access to view responses for this tenant
            var assignment = await dbContext.HrConsultantTenantAssignments
                .FirstOrDefaultAsync(a => a.HrConsultantId == consultantId
                    && a.TenantId == tenantId
                    && a.IsActive);

            if (assignment == null || !assignment.CanViewResponses)
            {
                return Results.Forbid();
            }

            var query = dbContext.RequestResponses
                .Include(r => r.RequestTypeVersion)
                    .ThenInclude(v => v!.RequestType)
                .Include(r => r.User)
                .Where(r => r.RequestTypeVersion!.RequestType!.TenantId == tenantId);

            if (isComplete.HasValue)
            {
                query = query.Where(r => r.IsComplete == isComplete.Value);
            }

            var totalCount = await query.CountAsync();

            var responses = await query
                .OrderByDescending(r => r.StartedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new HrResponseDto(
                    r.Id,
                    r.RequestTypeVersion!.RequestType!.Id,
                    r.RequestTypeVersion!.RequestType!.Name,
                    r.RequestTypeVersion!.RequestType!.Icon ?? "file-text",
                    r.UserId,
                    r.User!.Name,
                    r.User.Email,
                    r.RequestTypeVersion!.VersionNumber,
                    r.IsComplete,
                    r.StartedAt,
                    r.CompletedAt
                ))
                .ToListAsync();

            return Results.Ok(new HrResponsesListResponse(
                responses,
                totalCount,
                page,
                pageSize,
                (int)Math.Ceiling((double)totalCount / pageSize)
            ));
        })
        .WithName("GetHrTenantResponses")
        .WithDescription("Get form responses for a tenant")
        .Produces<HrResponsesListResponse>(200);

        group.MapGet("/tenants/{tenantId}/request-types", async (
            Guid tenantId,
            HttpContext httpContext,
            AppDbContext dbContext) =>
        {
            var consultantId = GetConsultantId(httpContext);
            if (consultantId == null)
            {
                return Results.Unauthorized();
            }

            // Verify consultant has access to this tenant
            var assignment = await dbContext.HrConsultantTenantAssignments
                .FirstOrDefaultAsync(a => a.HrConsultantId == consultantId
                    && a.TenantId == tenantId
                    && a.IsActive);

            if (assignment == null)
            {
                return Results.Forbid();
            }

            // Get request types for this tenant
            var requestTypes = await dbContext.RequestTypes
                .Where(rt => rt.TenantId == tenantId)
                .OrderBy(rt => rt.Name)
                .ToListAsync();

            // Get response counts per request type
            var responseCounts = await dbContext.RequestResponses
                .Include(r => r.RequestTypeVersion)
                .Where(r => r.RequestTypeVersion!.RequestType!.TenantId == tenantId)
                .GroupBy(r => r.RequestTypeVersion!.RequestTypeId)
                .Select(g => new {
                    RequestTypeId = g.Key,
                    TotalCount = g.Count(),
                    CompletedCount = g.Count(r => r.IsComplete)
                })
                .ToListAsync();

            var result = requestTypes.Select(rt => {
                var counts = responseCounts.FirstOrDefault(c => c.RequestTypeId == rt.Id);
                return new HrRequestTypeDto(
                    rt.Id,
                    rt.Name,
                    rt.Description,
                    rt.Icon ?? "file-text",
                    rt.CurrentVersionNumber,
                    rt.IsActive,
                    rt.CreatedAt,
                    rt.UpdatedAt,
                    counts?.TotalCount ?? 0,
                    counts?.CompletedCount ?? 0
                );
            }).ToList();

            return Results.Ok(result);
        })
        .WithName("GetHrTenantRequestTypes")
        .WithDescription("Get request types for a tenant")
        .Produces<List<HrRequestTypeDto>>(200);

        group.MapPost("/tenants/{tenantId}/request-types", async (
            Guid tenantId,
            HrCreateRequestTypeRequest request,
            HttpContext httpContext,
            AppDbContext dbContext) =>
        {
            var consultantId = GetConsultantId(httpContext);
            if (consultantId == null)
            {
                return Results.Unauthorized();
            }

            // Verify consultant has access to this tenant and can manage request types
            var assignment = await dbContext.HrConsultantTenantAssignments
                .FirstOrDefaultAsync(a => a.HrConsultantId == consultantId
                    && a.TenantId == tenantId
                    && a.IsActive);

            if (assignment == null || !assignment.CanManageRequestTypes)
            {
                return Results.Forbid();
            }

            // Verify the tenant exists
            var tenantExists = await dbContext.Tenants.AnyAsync(t => t.Id == tenantId);
            if (!tenantExists)
            {
                return Results.NotFound(new { error = "Tenant not found" });
            }

            // Create the request type
            var requestType = new RequestType
            {
                TenantId = tenantId,
                Name = request.Name,
                Description = request.Description,
                Icon = request.Icon ?? "file-text",
                CurrentVersionNumber = 1,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            dbContext.RequestTypes.Add(requestType);
            await dbContext.SaveChangesAsync();

            // Create the first version
            var version = new RequestTypeVersion
            {
                RequestTypeId = requestType.Id,
                VersionNumber = 1,
                FormJson = request.FormJson,
                CreatedAt = DateTime.UtcNow
            };

            dbContext.RequestTypeVersions.Add(version);
            await dbContext.SaveChangesAsync();

            // Update the request type to point to the active version
            requestType.ActiveVersionId = version.Id;
            await dbContext.SaveChangesAsync();

            return Results.Created($"/api/hr/tenants/{tenantId}/request-types/{requestType.Id}", new HrRequestTypeDto(
                requestType.Id,
                requestType.Name,
                requestType.Description,
                requestType.Icon ?? "file-text",
                requestType.CurrentVersionNumber,
                requestType.IsActive,
                requestType.CreatedAt,
                requestType.UpdatedAt,
                0,
                0
            ));
        })
        .WithName("CreateHrTenantRequestType")
        .WithDescription("Create a new request type for a tenant")
        .Produces<HrRequestTypeDto>(201)
        .Produces(403)
        .Produces(404);

        group.MapGet("/tenants/{tenantId}/request-types/{requestTypeId}", async (
            Guid tenantId,
            Guid requestTypeId,
            HttpContext httpContext,
            AppDbContext dbContext) =>
        {
            var consultantId = GetConsultantId(httpContext);
            if (consultantId == null)
            {
                return Results.Unauthorized();
            }

            // Verify consultant has access to this tenant
            var assignment = await dbContext.HrConsultantTenantAssignments
                .FirstOrDefaultAsync(a => a.HrConsultantId == consultantId
                    && a.TenantId == tenantId
                    && a.IsActive);

            if (assignment == null)
            {
                return Results.Forbid();
            }

            // Get the request type with all versions
            var requestType = await dbContext.RequestTypes
                .Include(rt => rt.Versions)
                .FirstOrDefaultAsync(rt => rt.Id == requestTypeId && rt.TenantId == tenantId);

            if (requestType == null)
            {
                return Results.NotFound(new { error = "Request type not found" });
            }

            // Get response counts per version
            var versionResponseCounts = await dbContext.RequestResponses
                .Where(r => r.RequestTypeVersion!.RequestTypeId == requestTypeId)
                .GroupBy(r => r.RequestTypeVersionId)
                .Select(g => new { VersionId = g.Key, Count = g.Count() })
                .ToListAsync();

            var versions = requestType.Versions
                .OrderByDescending(v => v.VersionNumber)
                .Select(v => new HrRequestTypeVersionDto(
                    v.Id,
                    v.VersionNumber,
                    v.FormJson,
                    v.CreatedAt,
                    versionResponseCounts.FirstOrDefault(c => c.VersionId == v.Id)?.Count ?? 0
                ))
                .ToList();

            var result = new HrRequestTypeDetailDto(
                requestType.Id,
                requestType.Name,
                requestType.Description,
                requestType.Icon ?? "file-text",
                requestType.CurrentVersionNumber,
                requestType.IsActive,
                requestType.CreatedAt,
                requestType.UpdatedAt,
                versions
            );

            return Results.Ok(result);
        })
        .WithName("GetHrTenantRequestTypeDetail")
        .WithDescription("Get request type details with version history")
        .Produces<HrRequestTypeDetailDto>(200);

        group.MapPut("/tenants/{tenantId}/request-types/{requestTypeId}", async (
            Guid tenantId,
            Guid requestTypeId,
            HrUpdateRequestTypeRequest request,
            HttpContext httpContext,
            AppDbContext dbContext) =>
        {
            var consultantId = GetConsultantId(httpContext);
            if (consultantId == null)
            {
                return Results.Unauthorized();
            }

            // Verify consultant has access to this tenant and can manage request types
            var assignment = await dbContext.HrConsultantTenantAssignments
                .FirstOrDefaultAsync(a => a.HrConsultantId == consultantId
                    && a.TenantId == tenantId
                    && a.IsActive);

            if (assignment == null || !assignment.CanManageRequestTypes)
            {
                return Results.Forbid();
            }

            // Get the request type and verify it belongs to this tenant
            var requestType = await dbContext.RequestTypes
                .Include(r => r.ActiveVersion)
                .FirstOrDefaultAsync(r => r.Id == requestTypeId && r.TenantId == tenantId);

            if (requestType == null)
            {
                return Results.NotFound(new { error = "Request type not found" });
            }

            // Check if formJson changed - if so, create a new version
            var formJsonChanged = requestType.ActiveVersion?.FormJson != request.FormJson;

            if (formJsonChanged)
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
            requestType.IsActive = request.IsActive;
            requestType.UpdatedAt = DateTime.UtcNow;

            await dbContext.SaveChangesAsync();

            // Reload to get the active version
            await dbContext.Entry(requestType).Reference(r => r.ActiveVersion).LoadAsync();

            return Results.Ok(new HrRequestTypeDetailDto(
                requestType.Id,
                requestType.Name,
                requestType.Description,
                requestType.Icon ?? "file-text",
                requestType.CurrentVersionNumber,
                requestType.IsActive,
                requestType.CreatedAt,
                requestType.UpdatedAt,
                new List<HrRequestTypeVersionDto>()
            ));
        })
        .WithName("UpdateHrTenantRequestType")
        .WithDescription("Update a request type for a tenant")
        .Produces<HrRequestTypeDetailDto>(200)
        .Produces(404);

        group.MapGet("/tenants/{tenantId}/responses/{responseId}", async (
            Guid tenantId,
            Guid responseId,
            HttpContext httpContext,
            AppDbContext dbContext) =>
        {
            var consultantId = GetConsultantId(httpContext);
            if (consultantId == null)
            {
                return Results.Unauthorized();
            }

            // Verify consultant has access to view responses for this tenant
            var assignment = await dbContext.HrConsultantTenantAssignments
                .FirstOrDefaultAsync(a => a.HrConsultantId == consultantId
                    && a.TenantId == tenantId
                    && a.IsActive);

            if (assignment == null || !assignment.CanViewResponses)
            {
                return Results.Forbid();
            }

            // Get the response with all related data
            var response = await dbContext.RequestResponses
                .Include(r => r.User)
                .Include(r => r.RequestTypeVersion)
                    .ThenInclude(v => v!.RequestType)
                .FirstOrDefaultAsync(r => r.Id == responseId
                    && r.RequestTypeVersion!.RequestType!.TenantId == tenantId);

            if (response == null)
            {
                return Results.NotFound(new { error = "Response not found" });
            }

            return Results.Ok(new HrResponseDetailDto(
                response.Id,
                response.RequestTypeVersion!.RequestType!.Id,
                response.RequestTypeVersion.RequestType.Name,
                response.RequestTypeVersion.RequestType.Icon ?? "file-text",
                response.UserId,
                response.User!.Name,
                response.User.Email,
                response.RequestTypeVersion.VersionNumber,
                response.ResponseJson,
                response.RequestTypeVersion.FormJson,
                response.IsComplete,
                response.StartedAt,
                response.CompletedAt
            ));
        })
        .WithName("GetHrTenantResponseDetail")
        .WithDescription("Get response details with form schema")
        .Produces<HrResponseDetailDto>(200)
        .Produces(404);
    }

    private static Guid? GetConsultantId(HttpContext httpContext)
    {
        var subClaim = httpContext.User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(subClaim) || !Guid.TryParse(subClaim, out var consultantId))
        {
            return null;
        }
        return consultantId;
    }
}

// DTOs for HR Consultant endpoints
public record HrDashboardStatsResponse(
    int TotalTenants,
    int TotalResponses,
    int PendingReview,
    decimal CompletionRate
);

public record TenantStatsResponse(
    int TotalUsers,
    int TotalResponses,
    int PendingResponses,
    int RequestTypesCount,
    decimal CompletionRate
);

public record HrResponseDto(
    Guid Id,
    Guid RequestTypeId,
    string RequestTypeName,
    string RequestTypeIcon,
    Guid UserId,
    string UserName,
    string UserEmail,
    int VersionNumber,
    bool IsComplete,
    DateTime StartedAt,
    DateTime? CompletedAt
);

public record HrResponsesListResponse(
    List<HrResponseDto> Responses,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);

public record HrRequestTypeDto(
    Guid Id,
    string Name,
    string? Description,
    string Icon,
    int CurrentVersionNumber,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    int TotalResponses,
    int CompletedResponses
);

public record HrRequestTypeDetailDto(
    Guid Id,
    string Name,
    string? Description,
    string Icon,
    int CurrentVersionNumber,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<HrRequestTypeVersionDto> Versions
);

public record HrRequestTypeVersionDto(
    Guid Id,
    int VersionNumber,
    string FormJson,
    DateTime CreatedAt,
    int ResponseCount
);

public record HrUpdateRequestTypeRequest(
    string Name,
    string? Description,
    string? Icon,
    string FormJson,
    bool IsActive
);

public record HrCreateRequestTypeRequest(
    string Name,
    string? Description,
    string? Icon,
    string FormJson
);

public record HrResponseDetailDto(
    Guid Id,
    Guid RequestTypeId,
    string RequestTypeName,
    string RequestTypeIcon,
    Guid UserId,
    string UserName,
    string UserEmail,
    int VersionNumber,
    string ResponseJson,
    string FormJson,
    bool IsComplete,
    DateTime StartedAt,
    DateTime? CompletedAt
);
