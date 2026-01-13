using Microsoft.EntityFrameworkCore;
using Portal.Api.Data;

namespace Portal.Api.Services;

public class OrphanFileCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OrphanFileCleanupService> _logger;
    private readonly TimeSpan _cleanupInterval = TimeSpan.FromHours(6);
    private readonly TimeSpan _orphanThreshold = TimeSpan.FromHours(24);

    public OrphanFileCleanupService(IServiceProvider serviceProvider, ILogger<OrphanFileCleanupService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Orphan file cleanup service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupOrphanedFilesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during orphan file cleanup");
            }

            await Task.Delay(_cleanupInterval, stoppingToken);
        }
    }

    private async Task CleanupOrphanedFilesAsync(CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var fileStorage = scope.ServiceProvider.GetRequiredService<IFileStorageService>();

        var threshold = DateTime.UtcNow - _orphanThreshold;

        // Find orphaned files (no linked response, uploaded more than 24 hours ago)
        // Need to ignore query filters to clean up files across all tenants
        var orphanedFiles = await dbContext.UploadedFiles
            .IgnoreQueryFilters()
            .Where(f => f.RequestResponseId == null && f.UploadedAt < threshold)
            .ToListAsync(ct);

        if (orphanedFiles.Count == 0)
        {
            _logger.LogDebug("No orphaned files to clean up");
            return;
        }

        _logger.LogInformation("Found {Count} orphaned files to clean up", orphanedFiles.Count);

        var deletedCount = 0;
        foreach (var file in orphanedFiles)
        {
            try
            {
                await fileStorage.DeleteFileAsync(file, ct);
                dbContext.UploadedFiles.Remove(file);
                deletedCount++;
                _logger.LogDebug("Deleted orphaned file: {FileId} ({FileName})", file.Id, file.OriginalFileName);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete orphaned file: {FileId}", file.Id);
            }
        }

        await dbContext.SaveChangesAsync(ct);
        _logger.LogInformation("Cleaned up {DeletedCount} orphaned files", deletedCount);
    }
}
