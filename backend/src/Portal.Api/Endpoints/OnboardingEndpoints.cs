using Microsoft.EntityFrameworkCore;
using Portal.Api.Data;
using Portal.Api.DTOs;
using Portal.Api.Models;
using Portal.Api.Services;

namespace Portal.Api.Endpoints;

public static class OnboardingEndpoints
{
    public static void MapOnboardingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/onboarding").WithTags("Onboarding");

        // Get onboarding status for current user
        group.MapGet("/status", async (ITenantContext tenantContext, AppDbContext dbContext, HttpContext httpContext) =>
        {
            var userIdClaim = httpContext.User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Results.Unauthorized();
            }

            var survey = await dbContext.OnboardingSurveys
                .AsNoTracking()
                .Include(s => s.ActiveVersion)
                .FirstOrDefaultAsync(s => s.IsActive && s.ActiveVersionId != null);

            if (survey?.ActiveVersion == null)
            {
                return Results.Ok(new OnboardingStatusDto(false, null));
            }

            var surveyDto = new OnboardingSurveyDto(
                survey.Id,
                survey.Name,
                survey.CurrentVersionNumber,
                survey.ActiveVersion.SurveyJson,
                survey.IsActive,
                survey.CreatedAt,
                survey.UpdatedAt
            );

            return Results.Ok(new OnboardingStatusDto(true, surveyDto));
        })
        .RequireAuthorization()
        .WithName("GetOnboardingStatus")
        .WithDescription("Get onboarding status for current user")
        .Produces<OnboardingStatusDto>(200);

        // Get active survey
        group.MapGet("/survey", async (AppDbContext dbContext) =>
        {
            var survey = await dbContext.OnboardingSurveys
                .AsNoTracking()
                .Include(s => s.ActiveVersion)
                .FirstOrDefaultAsync(s => s.IsActive && s.ActiveVersionId != null);

            if (survey?.ActiveVersion == null)
            {
                return Results.NotFound(new { error = "No active survey found" });
            }

            return Results.Ok(new OnboardingSurveyDto(
                survey.Id,
                survey.Name,
                survey.CurrentVersionNumber,
                survey.ActiveVersion.SurveyJson,
                survey.IsActive,
                survey.CreatedAt,
                survey.UpdatedAt
            ));
        })
        .RequireAuthorization()
        .WithName("GetOnboardingSurvey")
        .WithDescription("Get active onboarding survey")
        .Produces<OnboardingSurveyDto>(200)
        .Produces(404);

        // Get current user's responses
        group.MapGet("/response", async (AppDbContext dbContext, HttpContext httpContext) =>
        {
            var userIdClaim = httpContext.User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Results.Unauthorized();
            }

            var responses = await dbContext.OnboardingResponses
                .AsNoTracking()
                .Include(r => r.User)
                .Include(r => r.SurveyVersion)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.StartedAt)
                .Select(r => new OnboardingResponseDto(
                    r.Id,
                    r.UserId,
                    r.User.Name,
                    r.SurveyVersion.VersionNumber,
                    r.ResponseJson,
                    r.IsComplete,
                    r.StartedAt,
                    r.CompletedAt
                ))
                .ToListAsync();

            return Results.Ok(responses);
        })
        .RequireAuthorization()
        .WithName("GetOnboardingResponse")
        .WithDescription("Get current user's onboarding responses")
        .Produces<List<OnboardingResponseDto>>(200);

        // Submit new starter response
        group.MapPost("/response", async (SubmitOnboardingResponseRequest request, AppDbContext dbContext, HttpContext httpContext) =>
        {
            var userIdClaim = httpContext.User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Results.Unauthorized();
            }

            var survey = await dbContext.OnboardingSurveys
                .Include(s => s.ActiveVersion)
                .FirstOrDefaultAsync(s => s.IsActive && s.ActiveVersionId != null);

            if (survey?.ActiveVersion == null)
            {
                return Results.NotFound(new { error = "No active survey found" });
            }

            // Always create a new response - users can submit multiple new starters
            var newResponse = new OnboardingResponse
            {
                SurveyVersionId = survey.ActiveVersion.Id,
                UserId = userId,
                ResponseJson = request.ResponseJson,
                IsComplete = request.IsComplete,
                CompletedAt = request.IsComplete ? DateTime.UtcNow : null
            };
            dbContext.OnboardingResponses.Add(newResponse);

            await dbContext.SaveChangesAsync();

            return Results.Ok(new { message = "New starter submitted successfully" });
        })
        .RequireAuthorization()
        .WithName("SubmitOnboardingResponse")
        .WithDescription("Submit a new starter response")
        .Produces(200)
        .Produces(404);

        // Admin endpoints
        var adminGroup = app.MapGroup("/api/onboarding/admin").WithTags("Onboarding Admin");

        // Get survey config (admin)
        adminGroup.MapGet("/survey", async (AppDbContext dbContext) =>
        {
            var survey = await dbContext.OnboardingSurveys
                .AsNoTracking()
                .Include(s => s.ActiveVersion)
                .FirstOrDefaultAsync();

            if (survey == null)
            {
                return Results.NotFound(new { error = "No survey configured" });
            }

            return Results.Ok(new OnboardingSurveyDto(
                survey.Id,
                survey.Name,
                survey.CurrentVersionNumber,
                survey.ActiveVersion?.SurveyJson ?? string.Empty,
                survey.IsActive,
                survey.CreatedAt,
                survey.UpdatedAt
            ));
        })
        .RequireAuthorization("AdminOnly")
        .WithName("GetOnboardingSurveyAdmin")
        .WithDescription("Get survey configuration (admin only)")
        .Produces<OnboardingSurveyDto>(200)
        .Produces(404);

        // Create survey (admin)
        adminGroup.MapPost("/survey", async (CreateOnboardingSurveyRequest request, ITenantContext tenantContext, AppDbContext dbContext) =>
        {
            var existingSurvey = await dbContext.OnboardingSurveys.AnyAsync();
            if (existingSurvey)
            {
                return Results.BadRequest(new { error = "Survey already exists. Use PUT to update." });
            }

            // Create the survey
            var survey = new OnboardingSurvey
            {
                TenantId = tenantContext.TenantId,
                Name = request.Name,
                CurrentVersionNumber = 1,
                IsActive = true
            };

            dbContext.OnboardingSurveys.Add(survey);
            await dbContext.SaveChangesAsync();

            // Create the first version
            var version = new OnboardingSurveyVersion
            {
                SurveyId = survey.Id,
                VersionNumber = 1,
                SurveyJson = request.SurveyJson
            };

            dbContext.OnboardingSurveyVersions.Add(version);
            await dbContext.SaveChangesAsync();

            // Link the active version
            survey.ActiveVersionId = version.Id;
            await dbContext.SaveChangesAsync();

            return Results.Created($"/api/onboarding/admin/survey", new OnboardingSurveyDto(
                survey.Id,
                survey.Name,
                survey.CurrentVersionNumber,
                version.SurveyJson,
                survey.IsActive,
                survey.CreatedAt,
                survey.UpdatedAt
            ));
        })
        .RequireAuthorization("AdminOnly")
        .WithName("CreateOnboardingSurvey")
        .WithDescription("Create onboarding survey (admin only)")
        .Produces<OnboardingSurveyDto>(201)
        .Produces(400);

        // Update survey (admin) - creates a new version
        adminGroup.MapPut("/survey", async (UpdateOnboardingSurveyRequest request, AppDbContext dbContext) =>
        {
            var survey = await dbContext.OnboardingSurveys
                .Include(s => s.ActiveVersion)
                .FirstOrDefaultAsync();

            if (survey == null)
            {
                return Results.NotFound(new { error = "No survey found to update" });
            }

            // Check if surveyJson changed - if so, create a new version
            var surveyJsonChanged = survey.ActiveVersion?.SurveyJson != request.SurveyJson;

            if (surveyJsonChanged)
            {
                // Create a new version
                var newVersionNumber = survey.CurrentVersionNumber + 1;
                var newVersion = new OnboardingSurveyVersion
                {
                    SurveyId = survey.Id,
                    VersionNumber = newVersionNumber,
                    SurveyJson = request.SurveyJson
                };

                dbContext.OnboardingSurveyVersions.Add(newVersion);
                await dbContext.SaveChangesAsync();

                survey.CurrentVersionNumber = newVersionNumber;
                survey.ActiveVersionId = newVersion.Id;
            }

            survey.Name = request.Name;
            survey.IsActive = request.IsActive;
            survey.UpdatedAt = DateTime.UtcNow;

            await dbContext.SaveChangesAsync();

            // Reload to get the active version
            await dbContext.Entry(survey).Reference(s => s.ActiveVersion).LoadAsync();

            return Results.Ok(new OnboardingSurveyDto(
                survey.Id,
                survey.Name,
                survey.CurrentVersionNumber,
                survey.ActiveVersion?.SurveyJson ?? string.Empty,
                survey.IsActive,
                survey.CreatedAt,
                survey.UpdatedAt
            ));
        })
        .RequireAuthorization("AdminOnly")
        .WithName("UpdateOnboardingSurvey")
        .WithDescription("Update onboarding survey (admin only)")
        .Produces<OnboardingSurveyDto>(200)
        .Produces(404);

        // Get all responses (admin)
        adminGroup.MapGet("/responses", async (AppDbContext dbContext) =>
        {
            var responses = await dbContext.OnboardingResponses
                .AsNoTracking()
                .Include(r => r.User)
                .Include(r => r.SurveyVersion)
                .OrderByDescending(r => r.StartedAt)
                .Select(r => new OnboardingResponseDto(
                    r.Id,
                    r.UserId,
                    r.User.Name,
                    r.SurveyVersion.VersionNumber,
                    r.ResponseJson,
                    r.IsComplete,
                    r.StartedAt,
                    r.CompletedAt
                ))
                .ToListAsync();

            return Results.Ok(responses);
        })
        .RequireAuthorization("AdminOnly")
        .WithName("GetOnboardingResponses")
        .WithDescription("Get all onboarding responses (admin only)")
        .Produces<List<OnboardingResponseDto>>(200);
    }
}
