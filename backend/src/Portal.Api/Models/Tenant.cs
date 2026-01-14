namespace Portal.Api.Models;

public class Tenant
{
    public Guid Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Settings { get; set; }
    public string? Branding { get; set; }
    public string SubscriptionTier { get; set; } = "free";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<Invitation> Invitations { get; set; } = new List<Invitation>();
    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    public ICollection<HrConsultantTenantAssignment> ConsultantAssignments { get; set; } = new List<HrConsultantTenantAssignment>();
}
