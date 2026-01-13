namespace Portal.Api.Models;

public class OnboardingSurvey
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Name { get; set; } = "Onboarding Survey";
    public int CurrentVersionNumber { get; set; } = 0;
    public Guid? ActiveVersionId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Tenant Tenant { get; set; } = null!;
    public OnboardingSurveyVersion? ActiveVersion { get; set; }
    public ICollection<OnboardingSurveyVersion> Versions { get; set; } = new List<OnboardingSurveyVersion>();
}
