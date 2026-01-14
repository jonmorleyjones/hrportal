namespace Portal.Api.Models;

public class RefreshToken
{
    public Guid Id { get; set; }

    // Either UserId or HrConsultantId will be set, but not both
    public Guid? UserId { get; set; }
    public Guid? HrConsultantId { get; set; }

    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RevokedAt { get; set; }

    // Navigation properties
    public User? User { get; set; }
    public HrConsultant? HrConsultant { get; set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsRevoked => RevokedAt != null;
    public bool IsActive => !IsRevoked && !IsExpired;
}
