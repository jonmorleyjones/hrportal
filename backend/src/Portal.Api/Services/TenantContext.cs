namespace Portal.Api.Services;

public interface ITenantContext
{
    Guid TenantId { get; }
    string? TenantSlug { get; }
    void SetTenant(Guid tenantId, string? slug);
}

public class TenantContext : ITenantContext
{
    public Guid TenantId { get; private set; }
    public string? TenantSlug { get; private set; }

    public void SetTenant(Guid tenantId, string? slug)
    {
        TenantId = tenantId;
        TenantSlug = slug;
    }
}
