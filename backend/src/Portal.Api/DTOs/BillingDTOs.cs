namespace Portal.Api.DTOs;

public record SubscriptionResponse(
    string Tier,
    string Status,
    DateTime? CurrentPeriodStart,
    DateTime? CurrentPeriodEnd,
    decimal? MonthlyPrice,
    List<string> Features
);

public record InvoiceDto(
    Guid Id,
    string InvoiceNumber,
    decimal Amount,
    string Status,
    DateTime IssuedAt,
    DateTime? PaidAt
);

public record InvoiceListResponse(
    List<InvoiceDto> Invoices,
    int TotalCount
);

public record UpgradePlanRequest(string NewTier);
