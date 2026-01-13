namespace Portal.Api.Tests.Helpers;

public static class TestDatabaseFactory
{
    public static AppDbContext CreateInMemoryContext(ITenantContext tenantContext, string? dbName = null)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName ?? Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options, tenantContext);
    }

    public static (AppDbContext Context, ITenantContext TenantContext, Guid TenantId) CreateWithDefaultTenant(string? dbName = null)
    {
        var tenantContext = new TenantContext();
        var tenantId = Guid.NewGuid();
        tenantContext.SetTenant(tenantId, "test-tenant");

        var context = CreateInMemoryContext(tenantContext, dbName);

        // Seed default tenant
        context.Tenants.Add(new Tenant
        {
            Id = tenantId,
            Slug = "test-tenant",
            Name = "Test Tenant",
            IsActive = true,
            SubscriptionTier = "professional",
            CreatedAt = DateTime.UtcNow
        });
        context.SaveChanges();

        return (context, tenantContext, tenantId);
    }
}
