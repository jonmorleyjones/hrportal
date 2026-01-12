namespace Portal.Api.DTOs;

public record DashboardStatsResponse(
    int TotalUsers,
    int ActiveUsers,
    int PendingInvitations,
    decimal MonthlyActiveRate
);

public record ActivityItem(
    Guid Id,
    string Action,
    string EntityType,
    string? Description,
    string? UserName,
    DateTime CreatedAt
);

public record ActivityFeedResponse(
    List<ActivityItem> Activities,
    int TotalCount
);

public record ChartDataPoint(string Label, decimal Value);

public record ChartDataResponse(
    string ChartType,
    List<ChartDataPoint> Data
);
