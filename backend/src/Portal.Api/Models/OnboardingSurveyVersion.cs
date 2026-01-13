namespace Portal.Api.Models;

public class OnboardingSurveyVersion
{
    public Guid Id { get; set; }
    public Guid SurveyId { get; set; }
    public int VersionNumber { get; set; }
    public string SurveyJson { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public OnboardingSurvey Survey { get; set; } = null!;
    public ICollection<OnboardingResponse> Responses { get; set; } = new List<OnboardingResponse>();
}
