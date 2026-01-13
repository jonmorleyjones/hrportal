using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Portal.Api.Data;
using Portal.Api.DTOs;
using Portal.Api.Services;

namespace Portal.Api.Endpoints;

public static class FileEndpoints
{
    public static void MapFileEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/files").WithTags("Files");

        // Upload file
        group.MapPost("/upload", async (
            [FromQuery] string questionName,
            IFormFile file,
            IFileStorageService fileStorage,
            ITenantContext tenantContext,
            AppDbContext dbContext,
            HttpContext httpContext) =>
        {
            var userIdClaim = httpContext.User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Results.Unauthorized();
            }

            if (string.IsNullOrWhiteSpace(questionName))
            {
                return Results.BadRequest(new { error = "questionName is required" });
            }

            var validation = fileStorage.ValidateFile(file);
            if (!validation.isValid)
            {
                return Results.BadRequest(new { error = validation.error });
            }

            try
            {
                var uploadedFile = await fileStorage.SaveFileAsync(
                    tenantContext.TenantId,
                    userId,
                    questionName,
                    file
                );

                dbContext.UploadedFiles.Add(uploadedFile);
                await dbContext.SaveChangesAsync();

                var downloadUrl = $"/api/files/{uploadedFile.Id}";

                return Results.Created(downloadUrl, new FileUploadResponseDto(
                    uploadedFile.Id,
                    uploadedFile.OriginalFileName,
                    uploadedFile.ContentType,
                    uploadedFile.FileSizeBytes,
                    downloadUrl
                ));
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .RequireAuthorization()
        .DisableAntiforgery()
        .WithName("UploadFile")
        .WithDescription("Upload a file for a survey question")
        .Produces<FileUploadResponseDto>(201)
        .Produces(400)
        .Produces(401);

        // Download file
        group.MapGet("/{fileId:guid}", async (
            Guid fileId,
            AppDbContext dbContext,
            IFileStorageService fileStorage,
            HttpContext httpContext) =>
        {
            var userIdClaim = httpContext.User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Results.Unauthorized();
            }

            var userRole = httpContext.User.FindFirst("role")?.Value;

            var file = await dbContext.UploadedFiles
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.Id == fileId);

            if (file == null)
            {
                return Results.NotFound(new { error = "File not found" });
            }

            // Users can only access their own files, admins can access all
            if (file.UserId != userId && userRole != "Admin")
            {
                return Results.Forbid();
            }

            try
            {
                var stream = await fileStorage.GetFileStreamAsync(file);

                // Sanitize filename for Content-Disposition header
                var safeFileName = file.OriginalFileName.Replace("\"", "\\\"");

                return Results.File(
                    stream,
                    file.ContentType,
                    file.OriginalFileName,
                    enableRangeProcessing: true
                );
            }
            catch (FileNotFoundException)
            {
                return Results.NotFound(new { error = "File not found on storage" });
            }
        })
        .RequireAuthorization()
        .WithName("DownloadFile")
        .WithDescription("Download a file by ID")
        .Produces(200)
        .Produces(401)
        .Produces(403)
        .Produces(404);

        // Delete file (only unlinked files)
        group.MapDelete("/{fileId:guid}", async (
            Guid fileId,
            AppDbContext dbContext,
            IFileStorageService fileStorage,
            HttpContext httpContext) =>
        {
            var userIdClaim = httpContext.User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Results.Unauthorized();
            }

            var userRole = httpContext.User.FindFirst("role")?.Value;

            var file = await dbContext.UploadedFiles
                .FirstOrDefaultAsync(f => f.Id == fileId);

            if (file == null)
            {
                return Results.NotFound(new { error = "File not found" });
            }

            // Users can only delete their own unlinked files, admins can delete any
            if (file.UserId != userId && userRole != "Admin")
            {
                return Results.Forbid();
            }

            // Don't allow deleting files that are linked to a response (unless admin)
            if (file.RequestResponseId != null && userRole != "Admin")
            {
                return Results.BadRequest(new { error = "Cannot delete a file that is linked to a response" });
            }

            await fileStorage.DeleteFileAsync(file);
            dbContext.UploadedFiles.Remove(file);
            await dbContext.SaveChangesAsync();

            return Results.NoContent();
        })
        .RequireAuthorization()
        .WithName("DeleteFile")
        .WithDescription("Delete a file by ID")
        .Produces(204)
        .Produces(400)
        .Produces(401)
        .Produces(403)
        .Produces(404);

        // Link files to a response
        group.MapPost("/link", async (
            LinkFilesRequest request,
            AppDbContext dbContext,
            HttpContext httpContext) =>
        {
            var userIdClaim = httpContext.User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Results.Unauthorized();
            }

            // Verify the response exists and belongs to the user
            var response = await dbContext.RequestResponses
                .FirstOrDefaultAsync(r => r.Id == request.RequestResponseId);

            if (response == null)
            {
                return Results.NotFound(new { error = "Response not found" });
            }

            if (response.UserId != userId)
            {
                return Results.Forbid();
            }

            // Link the files
            var files = await dbContext.UploadedFiles
                .Where(f => request.FileIds.Contains(f.Id) && f.UserId == userId)
                .ToListAsync();

            foreach (var file in files)
            {
                file.RequestResponseId = request.RequestResponseId;
            }

            await dbContext.SaveChangesAsync();

            return Results.Ok(new { message = $"Linked {files.Count} files to response" });
        })
        .RequireAuthorization()
        .WithName("LinkFilesToResponse")
        .WithDescription("Link uploaded files to a response")
        .Produces(200)
        .Produces(401)
        .Produces(403)
        .Produces(404);

        // Get files for a response (admin)
        var adminGroup = app.MapGroup("/api/files/admin").WithTags("Files Admin");

        adminGroup.MapGet("/response/{responseId:guid}", async (
            Guid responseId,
            AppDbContext dbContext) =>
        {
            var files = await dbContext.UploadedFiles
                .AsNoTracking()
                .Where(f => f.RequestResponseId == responseId)
                .OrderBy(f => f.UploadedAt)
                .Select(f => new FileInfoDto(
                    f.Id,
                    f.OriginalFileName,
                    f.ContentType,
                    f.FileSizeBytes,
                    $"/api/files/{f.Id}",
                    f.UploadedAt
                ))
                .ToListAsync();

            return Results.Ok(files);
        })
        .RequireAuthorization("AdminOnly")
        .WithName("GetResponseFiles")
        .WithDescription("Get all files for a response (admin only)")
        .Produces<List<FileInfoDto>>(200);
    }
}
