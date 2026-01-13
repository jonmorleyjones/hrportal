using System.Security.Cryptography;

namespace Portal.Api.Tests.Helpers;

public class TestDataBuilder
{
    private readonly AppDbContext _context;
    private readonly Guid _tenantId;

    public TestDataBuilder(AppDbContext context, Guid tenantId)
    {
        _context = context;
        _tenantId = tenantId;
    }

    public User CreateUser(
        string email = "test@example.com",
        string name = "Test User",
        string password = "password123",
        UserRole role = UserRole.Member,
        bool isActive = true,
        Guid? id = null)
    {
        var user = new User
        {
            Id = id ?? Guid.NewGuid(),
            TenantId = _tenantId,
            Email = email,
            Name = name,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password, 4), // Lower work factor for tests
            Role = role,
            IsActive = isActive,
            CreatedAt = DateTime.UtcNow
        };
        _context.Users.Add(user);
        return user;
    }

    public RefreshToken CreateRefreshToken(
        Guid userId,
        string? token = null,
        DateTime? expiresAt = null,
        DateTime? revokedAt = null)
    {
        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = token ?? Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            ExpiresAt = expiresAt ?? DateTime.UtcNow.AddDays(7),
            RevokedAt = revokedAt,
            CreatedAt = DateTime.UtcNow
        };
        _context.RefreshTokens.Add(refreshToken);
        return refreshToken;
    }

    public Tenant CreateTenant(
        string slug = "test-tenant",
        string name = "Test Tenant",
        string subscriptionTier = "professional",
        bool isActive = true,
        Guid? id = null)
    {
        var tenant = new Tenant
        {
            Id = id ?? Guid.NewGuid(),
            Slug = slug,
            Name = name,
            SubscriptionTier = subscriptionTier,
            IsActive = isActive,
            CreatedAt = DateTime.UtcNow
        };
        _context.Tenants.Add(tenant);
        return tenant;
    }

    public void Save() => _context.SaveChanges();

    public async Task SaveAsync() => await _context.SaveChangesAsync();
}
