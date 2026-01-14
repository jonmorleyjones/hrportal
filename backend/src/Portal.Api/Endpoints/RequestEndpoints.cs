using Microsoft.EntityFrameworkCore;
using Portal.Api.Data;
using Portal.Api.DTOs;
using Portal.Api.Models;
using Portal.Api.Services;

namespace Portal.Api.Endpoints;

public static class RequestEndpoints
{
    public static void MapRequestEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/requests").WithTags("Requests");

        // List all active request types (for cards display)
        group.MapGet("/types", async (AppDbContext dbContext) =>
        {
            var requestTypes = await dbContext.RequestTypes
                .AsNoTracking()
                .Where(r => r.IsActive && r.ActiveVersionId != null)
                .OrderBy(r => r.Name)
                .Select(r => new RequestTypeCardDto(
                    r.Id,
                    r.Name,
                    r.Description,
                    r.Icon,
                    r.IsActive
                ))
                .ToListAsync();

            return Results.Ok(requestTypes);
        })
        .RequireAuthorization()
        .WithName("GetRequestTypes")
        .WithDescription("Get all active request types")
        .Produces<List<RequestTypeCardDto>>(200);

        // Get request type with form JSON
        group.MapGet("/types/{id:guid}", async (Guid id, AppDbContext dbContext) =>
        {
            var requestType = await dbContext.RequestTypes
                .AsNoTracking()
                .Include(r => r.ActiveVersion)
                .FirstOrDefaultAsync(r => r.Id == id && r.IsActive && r.ActiveVersionId != null);

            if (requestType?.ActiveVersion == null)
            {
                return Results.NotFound(new { error = "Request type not found" });
            }

            return Results.Ok(new RequestTypeDto(
                requestType.Id,
                requestType.Name,
                requestType.Description,
                requestType.Icon,
                requestType.CurrentVersionNumber,
                requestType.ActiveVersion.FormJson,
                requestType.IsActive,
                requestType.CreatedAt,
                requestType.UpdatedAt
            ));
        })
        .RequireAuthorization()
        .WithName("GetRequestType")
        .WithDescription("Get request type with form JSON")
        .Produces<RequestTypeDto>(200)
        .Produces(404);

        // Submit response for a request type
        group.MapPost("/types/{id:guid}/responses", async (Guid id, SubmitRequestResponseRequest request, AppDbContext dbContext, HttpContext httpContext) =>
        {
            var userIdClaim = httpContext.User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Results.Unauthorized();
            }

            var requestType = await dbContext.RequestTypes
                .Include(r => r.ActiveVersion)
                .FirstOrDefaultAsync(r => r.Id == id && r.IsActive && r.ActiveVersionId != null);

            if (requestType?.ActiveVersion == null)
            {
                return Results.NotFound(new { error = "Request type not found" });
            }

            var newResponse = new RequestResponse
            {
                RequestTypeVersionId = requestType.ActiveVersion.Id,
                UserId = userId,
                ResponseJson = request.ResponseJson,
                IsComplete = request.IsComplete,
                CompletedAt = request.IsComplete ? DateTime.UtcNow : null
            };
            dbContext.RequestResponses.Add(newResponse);

            await dbContext.SaveChangesAsync();

            return Results.Ok(new { message = "Response submitted successfully", id = newResponse.Id });
        })
        .RequireAuthorization()
        .WithName("SubmitRequestResponse")
        .WithDescription("Submit a response for a request type")
        .Produces(200)
        .Produces(404);

        // Get current user's responses
        group.MapGet("/responses", async (AppDbContext dbContext, HttpContext httpContext) =>
        {
            var userIdClaim = httpContext.User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Results.Unauthorized();
            }

            var responses = await dbContext.RequestResponses
                .AsNoTracking()
                .Include(r => r.User)
                .Include(r => r.RequestTypeVersion)
                    .ThenInclude(v => v.RequestType)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.StartedAt)
                .Select(r => new RequestResponseDto(
                    r.Id,
                    r.RequestTypeVersion.RequestType.Id,
                    r.RequestTypeVersion.RequestType.Name,
                    r.RequestTypeVersion.RequestType.Icon,
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
        .RequireAuthorization()
        .WithName("GetUserRequestResponses")
        .WithDescription("Get current user's request responses")
        .Produces<List<RequestResponseDto>>(200);

        // Admin endpoints
        var adminGroup = app.MapGroup("/api/requests/admin").WithTags("Requests Admin");

        // List all request types (admin - includes inactive)
        adminGroup.MapGet("/types", async (AppDbContext dbContext) =>
        {
            var requestTypes = await dbContext.RequestTypes
                .AsNoTracking()
                .Include(r => r.ActiveVersion)
                .OrderBy(r => r.Name)
                .Select(r => new RequestTypeDto(
                    r.Id,
                    r.Name,
                    r.Description,
                    r.Icon,
                    r.CurrentVersionNumber,
                    r.ActiveVersion != null ? r.ActiveVersion.FormJson : string.Empty,
                    r.IsActive,
                    r.CreatedAt,
                    r.UpdatedAt
                ))
                .ToListAsync();

            return Results.Ok(requestTypes);
        })
        .RequireAuthorization("AdminOnly")
        .WithName("GetRequestTypesAdmin")
        .WithDescription("Get all request types (admin only)")
        .Produces<List<RequestTypeDto>>(200);

        // Create request type (admin)
        adminGroup.MapPost("/types", async (CreateRequestTypeRequest request, ITenantContext tenantContext, AppDbContext dbContext) =>
        {
            var requestType = new RequestType
            {
                TenantId = tenantContext.TenantId,
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

            return Results.Created($"/api/requests/types/{requestType.Id}", new RequestTypeDto(
                requestType.Id,
                requestType.Name,
                requestType.Description,
                requestType.Icon,
                requestType.CurrentVersionNumber,
                version.FormJson,
                requestType.IsActive,
                requestType.CreatedAt,
                requestType.UpdatedAt
            ));
        })
        .RequireAuthorization("AdminOnly")
        .WithName("CreateRequestType")
        .WithDescription("Create a new request type (admin only)")
        .Produces<RequestTypeDto>(201);

        // Update request type (admin) - creates a new version if form JSON changed
        adminGroup.MapPut("/types/{id:guid}", async (Guid id, UpdateRequestTypeRequest request, AppDbContext dbContext) =>
        {
            var requestType = await dbContext.RequestTypes
                .Include(r => r.ActiveVersion)
                .FirstOrDefaultAsync(r => r.Id == id);

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
        .RequireAuthorization("AdminOnly")
        .WithName("UpdateRequestType")
        .WithDescription("Update a request type (admin only)")
        .Produces<RequestTypeDto>(200)
        .Produces(404);

        // Delete request type (admin) - soft delete by setting IsActive to false
        adminGroup.MapDelete("/types/{id:guid}", async (Guid id, AppDbContext dbContext) =>
        {
            var requestType = await dbContext.RequestTypes.FirstOrDefaultAsync(r => r.Id == id);

            if (requestType == null)
            {
                return Results.NotFound(new { error = "Request type not found" });
            }

            requestType.IsActive = false;
            requestType.UpdatedAt = DateTime.UtcNow;
            await dbContext.SaveChangesAsync();

            return Results.Ok(new { message = "Request type deleted" });
        })
        .RequireAuthorization("AdminOnly")
        .WithName("DeleteRequestType")
        .WithDescription("Delete a request type (admin only)")
        .Produces(200)
        .Produces(404);

        // Get responses for a specific request type (admin)
        adminGroup.MapGet("/types/{id:guid}/responses", async (Guid id, AppDbContext dbContext) =>
        {
            var requestTypeExists = await dbContext.RequestTypes.AnyAsync(r => r.Id == id);
            if (!requestTypeExists)
            {
                return Results.NotFound(new { error = "Request type not found" });
            }

            var responses = await dbContext.RequestResponses
                .AsNoTracking()
                .Include(r => r.User)
                .Include(r => r.RequestTypeVersion)
                    .ThenInclude(v => v.RequestType)
                .Where(r => r.RequestTypeVersion.RequestTypeId == id)
                .OrderByDescending(r => r.StartedAt)
                .Select(r => new RequestResponseDto(
                    r.Id,
                    r.RequestTypeVersion.RequestType.Id,
                    r.RequestTypeVersion.RequestType.Name,
                    r.RequestTypeVersion.RequestType.Icon,
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
        .RequireAuthorization("AdminOnly")
        .WithName("GetRequestTypeResponses")
        .WithDescription("Get responses for a request type (admin only)")
        .Produces<List<RequestResponseDto>>(200)
        .Produces(404);

        // Get all responses (admin)
        adminGroup.MapGet("/responses", async (AppDbContext dbContext) =>
        {
            var responses = await dbContext.RequestResponses
                .AsNoTracking()
                .Include(r => r.User)
                .Include(r => r.RequestTypeVersion)
                    .ThenInclude(v => v.RequestType)
                .OrderByDescending(r => r.StartedAt)
                .Select(r => new RequestResponseDto(
                    r.Id,
                    r.RequestTypeVersion.RequestType.Id,
                    r.RequestTypeVersion.RequestType.Name,
                    r.RequestTypeVersion.RequestType.Icon,
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
        .RequireAuthorization("AdminOnly")
        .WithName("GetAllRequestResponses")
        .WithDescription("Get all request responses (admin only)")
        .Produces<List<RequestResponseDto>>(200);
    }
}
