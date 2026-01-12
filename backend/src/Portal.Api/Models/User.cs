namespace Portal.Api.Models;

public class User
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Member;
    public Guid? InvitedBy { get; set; }
    public DateTime? InvitedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    public Tenant Tenant { get; set; } = null!;
    public User? InvitedByUser { get; set; }
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}

public enum UserRole
{
    Viewer,
    Member,
    Admin
}
