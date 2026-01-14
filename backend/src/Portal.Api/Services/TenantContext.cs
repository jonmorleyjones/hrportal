namespace Portal.Api.Services;

public interface ITenantContext
{
    Guid TenantId { get; }
    string? TenantSlug { get; }

    // Consultant mode properties
    bool IsConsultantMode { get; }
    Guid? ConsultantId { get; }
    IReadOnlyList<Guid>? AssignedTenantIds { get; }

    void SetTenant(Guid tenantId, string? slug);
    void SetConsultantMode(Guid consultantId, IEnumerable<Guid> assignedTenantIds);
    void SetActiveTenant(Guid tenantId, string? slug);
    void ClearConsultantMode();
}

public class TenantContext : ITenantContext
{
    public Guid TenantId { get; private set; }
    public string? TenantSlug { get; private set; }

    // Consultant mode properties
    public bool IsConsultantMode { get; private set; }
    public Guid? ConsultantId { get; private set; }
    public IReadOnlyList<Guid>? AssignedTenantIds { get; private set; }

    public void SetTenant(Guid tenantId, string? slug)
    {
        TenantId = tenantId;
        TenantSlug = slug;
    }

    public void SetConsultantMode(Guid consultantId, IEnumerable<Guid> assignedTenantIds)
    {
        IsConsultantMode = true;
        ConsultantId = consultantId;
        AssignedTenantIds = assignedTenantIds.ToList().AsReadOnly();
        // In consultant mode, TenantId is empty by default (cross-tenant view)
        TenantId = Guid.Empty;
        TenantSlug = null;
    }

    public void SetActiveTenant(Guid tenantId, string? slug)
    {
        if (!IsConsultantMode)
        {
            throw new InvalidOperationException("Cannot set active tenant outside of consultant mode");
        }

        if (AssignedTenantIds != null && !AssignedTenantIds.Contains(tenantId))
        {
            throw new UnauthorizedAccessException("Consultant does not have access to this tenant");
        }

        TenantId = tenantId;
        TenantSlug = slug;
    }

    public void ClearConsultantMode()
    {
        IsConsultantMode = false;
        ConsultantId = null;
        AssignedTenantIds = null;
    }
}
