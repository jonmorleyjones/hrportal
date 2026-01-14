using Microsoft.EntityFrameworkCore;
using Portal.Api.Data;
using Portal.Api.DTOs;
using Portal.Api.Models;

namespace Portal.Api.Services;

public interface IHrConsultantAuthService
{
    Task<ConsultantLoginResponse?> LoginAsync(string email, string password);
    Task<ConsultantRefreshResponse?> RefreshTokenAsync(string refreshToken);
    Task<bool> LogoutAsync(string refreshToken);
    Task<IList<TenantSummaryDto>> GetAssignedTenantsAsync(Guid consultantId);
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
}

public class HrConsultantAuthService : IHrConsultantAuthService
{
    private readonly AppDbContext _dbContext;
    private readonly IJwtService _jwtService;
    private readonly IConfiguration _configuration;

    public HrConsultantAuthService(AppDbContext dbContext, IJwtService jwtService, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _jwtService = jwtService;
        _configuration = configuration;
    }

    public async Task<ConsultantLoginResponse?> LoginAsync(string email, string password)
    {
        // Query without tenant filter (consultants are not tenant-scoped)
        var consultant = await _dbContext.HrConsultants
            .FirstOrDefaultAsync(c => c.Email == email && c.IsActive);

        if (consultant == null || !VerifyPassword(password, consultant.PasswordHash))
        {
            return null;
        }

        // Update last login
        consultant.LastLoginAt = DateTime.UtcNow;

        // Generate tokens
        var accessToken = _jwtService.GenerateConsultantAccessToken(consultant);
        var refreshToken = _jwtService.GenerateRefreshToken();

        // Store refresh token
        var refreshTokenDays = int.Parse(_configuration["Jwt:RefreshTokenExpiryDays"] ?? "7");
        var tokenEntity = new RefreshToken
        {
            HrConsultantId = consultant.Id,
            Token = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(refreshTokenDays)
        };
        _dbContext.RefreshTokens.Add(tokenEntity);

        await _dbContext.SaveChangesAsync();

        // Get assigned tenants
        var assignedTenants = await GetAssignedTenantsAsync(consultant.Id);

        return new ConsultantLoginResponse(
            accessToken,
            refreshToken,
            new ConsultantDto(consultant.Id, consultant.Email, consultant.Name, consultant.LastLoginAt, consultant.IsActive),
            assignedTenants
        );
    }

    public async Task<ConsultantRefreshResponse?> RefreshTokenAsync(string refreshToken)
    {
        // Query refresh tokens without tenant filter for consultant tokens
        var storedToken = await _dbContext.RefreshTokens
            .IgnoreQueryFilters()
            .Include(rt => rt.HrConsultant)
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken && rt.HrConsultantId != null);

        if (storedToken == null || !storedToken.IsActive || storedToken.HrConsultant == null || !storedToken.HrConsultant.IsActive)
        {
            return null;
        }

        // Revoke old token
        storedToken.RevokedAt = DateTime.UtcNow;

        // Generate new tokens
        var newAccessToken = _jwtService.GenerateConsultantAccessToken(storedToken.HrConsultant);
        var newRefreshToken = _jwtService.GenerateRefreshToken();

        // Store new refresh token
        var refreshTokenDays = int.Parse(_configuration["Jwt:RefreshTokenExpiryDays"] ?? "7");
        var newTokenEntity = new RefreshToken
        {
            HrConsultantId = storedToken.HrConsultantId,
            Token = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(refreshTokenDays)
        };
        _dbContext.RefreshTokens.Add(newTokenEntity);

        await _dbContext.SaveChangesAsync();

        return new ConsultantRefreshResponse(newAccessToken, newRefreshToken);
    }

    public async Task<bool> LogoutAsync(string refreshToken)
    {
        var storedToken = await _dbContext.RefreshTokens
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken && rt.HrConsultantId != null);

        if (storedToken == null)
        {
            return false;
        }

        storedToken.RevokedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        return true;
    }

    public async Task<IList<TenantSummaryDto>> GetAssignedTenantsAsync(Guid consultantId)
    {
        var assignments = await _dbContext.HrConsultantTenantAssignments
            .Include(a => a.Tenant)
            .Where(a => a.HrConsultantId == consultantId && a.IsActive && a.Tenant.IsActive)
            .ToListAsync();

        var tenantSummaries = new List<TenantSummaryDto>();

        foreach (var assignment in assignments)
        {
            // Get stats for each tenant
            var userCount = await _dbContext.Users
                .IgnoreQueryFilters()
                .CountAsync(u => u.TenantId == assignment.TenantId && u.IsActive);

            var activeRequestTypes = await _dbContext.RequestTypes
                .IgnoreQueryFilters()
                .CountAsync(r => r.TenantId == assignment.TenantId && r.IsActive);

            var pendingResponses = await _dbContext.RequestResponses
                .IgnoreQueryFilters()
                .Include(r => r.RequestTypeVersion)
                    .ThenInclude(v => v.RequestType)
                .CountAsync(r => r.RequestTypeVersion.RequestType.TenantId == assignment.TenantId && !r.IsComplete);

            tenantSummaries.Add(new TenantSummaryDto(
                assignment.Tenant.Id,
                assignment.Tenant.Slug,
                assignment.Tenant.Name,
                assignment.Tenant.SubscriptionTier,
                userCount,
                activeRequestTypes,
                pendingResponses,
                assignment.CanManageRequestTypes,
                assignment.CanManageSettings,
                assignment.CanManageBranding,
                assignment.CanViewResponses
            ));
        }

        return tenantSummaries;
    }

    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public bool VerifyPassword(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }
}
