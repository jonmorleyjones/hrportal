namespace Portal.Api.Models;

public class RequestType
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Name { get; set; } = "Request";
    public string? Description { get; set; }
    public string Icon { get; set; } = "clipboard-list";
    public int CurrentVersionNumber { get; set; } = 0;
    public Guid? ActiveVersionId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Tenant Tenant { get; set; } = null!;
    public RequestTypeVersion? ActiveVersion { get; set; }
    public ICollection<RequestTypeVersion> Versions { get; set; } = new List<RequestTypeVersion>();
}
