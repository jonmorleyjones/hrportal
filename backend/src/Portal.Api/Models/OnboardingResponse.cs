namespace Portal.Api.Models;

public class OnboardingResponse
{
    public Guid Id { get; set; }
    public Guid SurveyVersionId { get; set; }
    public Guid UserId { get; set; }
    public string ResponseJson { get; set; } = string.Empty;
    public bool IsComplete { get; set; } = false;
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }

    public OnboardingSurveyVersion SurveyVersion { get; set; } = null!;
    public User User { get; set; } = null!;
}
