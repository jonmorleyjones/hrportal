using Microsoft.EntityFrameworkCore;
using Portal.Api.Data;
using Portal.Api.DTOs;
using Portal.Api.Models;

namespace Portal.Api.Services;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(string email, string password, Guid tenantId);
    Task<RefreshResponse?> RefreshTokenAsync(string refreshToken);
    Task<bool> LogoutAsync(string refreshToken);
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
}

public class AuthService : IAuthService
{
    private readonly AppDbContext _dbContext;
    private readonly IJwtService _jwtService;
    private readonly IConfiguration _configuration;

    public AuthService(AppDbContext dbContext, IJwtService jwtService, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _jwtService = jwtService;
        _configuration = configuration;
    }

    public async Task<LoginResponse?> LoginAsync(string email, string password, Guid tenantId)
    {
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == email && u.TenantId == tenantId && u.IsActive);

        if (user == null || !VerifyPassword(password, user.PasswordHash))
        {
            return null;
        }

        // Update last login
        user.LastLoginAt = DateTime.UtcNow;

        // Generate tokens
        var accessToken = _jwtService.GenerateAccessToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken();

        // Store refresh token
        var refreshTokenDays = int.Parse(_configuration["Jwt:RefreshTokenExpiryDays"] ?? "7");
        var tokenEntity = new RefreshToken
        {
            UserId = user.Id,
            Token = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(refreshTokenDays)
        };
        _dbContext.RefreshTokens.Add(tokenEntity);

        await _dbContext.SaveChangesAsync();

        return new LoginResponse(
            accessToken,
            refreshToken,
            new UserDto(user.Id, user.Email, user.Name, user.Role, user.LastLoginAt, user.IsActive)
        );
    }

    public async Task<RefreshResponse?> RefreshTokenAsync(string refreshToken)
    {
        var storedToken = await _dbContext.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

        if (storedToken == null || !storedToken.IsActive || !storedToken.User.IsActive)
        {
            return null;
        }

        // Revoke old token
        storedToken.RevokedAt = DateTime.UtcNow;

        // Generate new tokens
        var newAccessToken = _jwtService.GenerateAccessToken(storedToken.User);
        var newRefreshToken = _jwtService.GenerateRefreshToken();

        // Store new refresh token
        var refreshTokenDays = int.Parse(_configuration["Jwt:RefreshTokenExpiryDays"] ?? "7");
        var newTokenEntity = new RefreshToken
        {
            UserId = storedToken.UserId,
            Token = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(refreshTokenDays)
        };
        _dbContext.RefreshTokens.Add(newTokenEntity);

        await _dbContext.SaveChangesAsync();

        return new RefreshResponse(newAccessToken, newRefreshToken);
    }

    public async Task<bool> LogoutAsync(string refreshToken)
    {
        var storedToken = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

        if (storedToken == null)
        {
            return false;
        }

        storedToken.RevokedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        return true;
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
