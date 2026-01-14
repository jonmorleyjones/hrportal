namespace Portal.Api.Models;

public class HrConsultantTenantAssignment
{
    public Guid Id { get; set; }
    public Guid HrConsultantId { get; set; }
    public Guid TenantId { get; set; }
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    // Granular permissions for this tenant assignment
    public bool CanManageRequestTypes { get; set; } = true;
    public bool CanManageSettings { get; set; } = true;
    public bool CanManageBranding { get; set; } = true;
    public bool CanViewResponses { get; set; } = true;

    // Navigation properties
    public HrConsultant HrConsultant { get; set; } = null!;
    public Tenant Tenant { get; set; } = null!;
}
