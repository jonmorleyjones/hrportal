using Microsoft.EntityFrameworkCore;
using Portal.Api.Data;
using Portal.Api.DTOs;
using Portal.Api.Services;

namespace Portal.Api.Endpoints;

public static class BillingEndpoints
{
    public static void MapBillingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/billing")
            .WithTags("Billing")
            .RequireAuthorization();

        group.MapGet("/subscription", async (ITenantContext tenantContext, AppDbContext dbContext) =>
        {
            var tenant = await dbContext.Tenants
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == tenantContext.TenantId);

            if (tenant == null)
            {
                return Results.NotFound(new { error = "Tenant not found" });
            }

            var features = tenant.SubscriptionTier.ToLower() switch
            {
                "free" => new List<string> { "Up to 5 users", "Basic dashboard", "Email support" },
                "starter" => new List<string> { "Up to 25 users", "Full dashboard", "Priority email support", "API access" },
                "professional" => new List<string> { "Up to 100 users", "Advanced analytics", "Phone support", "API access", "Custom branding" },
                "enterprise" => new List<string> { "Unlimited users", "Advanced analytics", "Dedicated support", "API access", "Custom branding", "SSO", "SLA" },
                _ => new List<string>()
            };

            var monthlyPrice = tenant.SubscriptionTier.ToLower() switch
            {
                "free" => 0m,
                "starter" => 29m,
                "professional" => 99m,
                "enterprise" => 299m,
                _ => 0m
            };

            return Results.Ok(new SubscriptionResponse(
                tenant.SubscriptionTier,
                "active",
                DateTime.UtcNow.AddDays(-15),
                DateTime.UtcNow.AddDays(15),
                monthlyPrice,
                features
            ));
        })
        .WithName("GetSubscription")
        .WithDescription("Get current subscription details")
        .Produces<SubscriptionResponse>(200)
        .Produces(404);

        group.MapGet("/invoices", (int page, int pageSize) =>
        {
            // Mock invoice data - in production, this would come from a billing provider
            var invoices = new List<InvoiceDto>
            {
                new(Guid.NewGuid(), "INV-2024-001", 99.00m, "paid", DateTime.UtcNow.AddMonths(-1), DateTime.UtcNow.AddMonths(-1).AddDays(2)),
                new(Guid.NewGuid(), "INV-2024-002", 99.00m, "paid", DateTime.UtcNow.AddMonths(-2), DateTime.UtcNow.AddMonths(-2).AddDays(1)),
                new(Guid.NewGuid(), "INV-2024-003", 99.00m, "paid", DateTime.UtcNow.AddMonths(-3), DateTime.UtcNow.AddMonths(-3).AddDays(3))
            };

            return Results.Ok(new InvoiceListResponse(invoices, invoices.Count));
        })
        .WithName("GetInvoices")
        .WithDescription("Get invoice history")
        .Produces<InvoiceListResponse>(200);

        group.MapPost("/upgrade", async (UpgradePlanRequest request, ITenantContext tenantContext, AppDbContext dbContext) =>
        {
            var validTiers = new[] { "free", "starter", "professional", "enterprise" };
            if (!validTiers.Contains(request.NewTier.ToLower()))
            {
                return Results.BadRequest(new { error = "Invalid subscription tier" });
            }

            var tenant = await dbContext.Tenants
                .FirstOrDefaultAsync(t => t.Id == tenantContext.TenantId);

            if (tenant == null)
            {
                return Results.NotFound(new { error = "Tenant not found" });
            }

            tenant.SubscriptionTier = request.NewTier.ToLower();
            await dbContext.SaveChangesAsync();

            // TODO: Integrate with payment provider

            return Results.Ok(new { message = $"Subscription upgraded to {request.NewTier}" });
        })
        .RequireAuthorization("AdminOnly")
        .WithName("UpgradePlan")
        .WithDescription("Upgrade subscription plan (admin only)")
        .Produces(200)
        .Produces(400)
        .Produces(404);
    }
}
