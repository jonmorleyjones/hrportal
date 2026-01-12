using Microsoft.EntityFrameworkCore;
using Portal.Api.Data;
using Portal.Api.DTOs;

namespace Portal.Api.Endpoints;

public static class DashboardEndpoints
{
    public static void MapDashboardEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/dashboard")
            .WithTags("Dashboard")
            .RequireAuthorization();

        group.MapGet("/stats", async (AppDbContext dbContext) =>
        {
            var totalUsers = await dbContext.Users.CountAsync(u => u.IsActive);
            var activeUsers = await dbContext.Users.CountAsync(u => u.IsActive && u.LastLoginAt > DateTime.UtcNow.AddDays(-30));
            var pendingInvitations = await dbContext.Invitations.CountAsync(i => i.AcceptedAt == null && i.ExpiresAt > DateTime.UtcNow);

            var monthlyActiveRate = totalUsers > 0 ? (decimal)activeUsers / totalUsers * 100 : 0;

            return Results.Ok(new DashboardStatsResponse(
                totalUsers,
                activeUsers,
                pendingInvitations,
                Math.Round(monthlyActiveRate, 1)
            ));
        })
        .WithName("GetDashboardStats")
        .WithDescription("Get dashboard KPI statistics")
        .Produces<DashboardStatsResponse>(200);

        group.MapGet("/activity", async (int limit, AppDbContext dbContext) =>
        {
            limit = limit < 1 ? 10 : limit > 50 ? 50 : limit;

            var activities = await dbContext.AuditLogs
                .AsNoTracking()
                .OrderByDescending(a => a.CreatedAt)
                .Take(limit)
                .Select(a => new ActivityItem(
                    a.Id,
                    a.Action,
                    a.EntityType,
                    a.Metadata,
                    a.User != null ? a.User.Name : null,
                    a.CreatedAt
                ))
                .ToListAsync();

            var totalCount = await dbContext.AuditLogs.CountAsync();

            return Results.Ok(new ActivityFeedResponse(activities, totalCount));
        })
        .WithName("GetActivityFeed")
        .WithDescription("Get recent activity feed")
        .Produces<ActivityFeedResponse>(200);

        group.MapGet("/charts/{chartType}", (string chartType) =>
        {
            // Mock chart data - in production, this would query real data
            var data = chartType.ToLower() switch
            {
                "users" => new List<ChartDataPoint>
                {
                    new("Jan", 10),
                    new("Feb", 15),
                    new("Mar", 22),
                    new("Apr", 28),
                    new("May", 35),
                    new("Jun", 42)
                },
                "activity" => new List<ChartDataPoint>
                {
                    new("Mon", 120),
                    new("Tue", 150),
                    new("Wed", 180),
                    new("Thu", 140),
                    new("Fri", 200),
                    new("Sat", 80),
                    new("Sun", 60)
                },
                _ => new List<ChartDataPoint>()
            };

            return Results.Ok(new ChartDataResponse(chartType, data));
        })
        .WithName("GetChartData")
        .WithDescription("Get chart data by type")
        .Produces<ChartDataResponse>(200);
    }
}
