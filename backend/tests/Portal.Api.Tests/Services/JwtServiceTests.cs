using System.IdentityModel.Tokens.Jwt;
using Portal.Api.Tests.Helpers;

namespace Portal.Api.Tests.Services;

public class JwtServiceTests
{
    private readonly IConfiguration _configuration;
    private readonly JwtService _sut;

    public JwtServiceTests()
    {
        _configuration = MockConfiguration.CreateWithJwtSettings();
        _sut = new JwtService(_configuration);
    }

    #region GenerateAccessToken Tests

    [Fact]
    public void GenerateAccessToken_WithValidUser_ReturnsNonEmptyToken()
    {
        // Arrange
        var user = CreateTestUser();

        // Act
        var token = _sut.GenerateAccessToken(user);

        // Assert
        token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GenerateAccessToken_WithValidUser_ContainsSubjectClaim()
    {
        // Arrange
        var user = CreateTestUser();

        // Act
        var token = _sut.GenerateAccessToken(user);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        // Assert
        jwt.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == user.Id.ToString());
    }

    [Fact]
    public void GenerateAccessToken_WithValidUser_ContainsEmailClaim()
    {
        // Arrange
        var user = CreateTestUser();

        // Act
        var token = _sut.GenerateAccessToken(user);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        // Assert
        jwt.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == user.Email);
    }

    [Fact]
    public void GenerateAccessToken_WithValidUser_ContainsRoleClaim()
    {
        // Arrange
        var user = CreateTestUser(role: UserRole.Admin);

        // Act
        var token = _sut.GenerateAccessToken(user);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        // Assert
        jwt.Claims.Should().Contain(c => c.Type == "role" && c.Value == "Admin");
    }

    [Fact]
    public void GenerateAccessToken_WithValidUser_ContainsTenantIdClaim()
    {
        // Arrange
        var user = CreateTestUser();

        // Act
        var token = _sut.GenerateAccessToken(user);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        // Assert
        jwt.Claims.Should().Contain(c => c.Type == "tenant_id" && c.Value == user.TenantId.ToString());
    }

    [Fact]
    public void GenerateAccessToken_WithValidUser_ContainsNameClaim()
    {
        // Arrange
        var user = CreateTestUser();

        // Act
        var token = _sut.GenerateAccessToken(user);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        // Assert
        jwt.Claims.Should().Contain(c => c.Type == "name" && c.Value == user.Name);
    }

    [Fact]
    public void GenerateAccessToken_WithValidUser_HasCorrectIssuer()
    {
        // Arrange
        var user = CreateTestUser();

        // Act
        var token = _sut.GenerateAccessToken(user);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        // Assert
        jwt.Issuer.Should().Be("test-issuer");
    }

    [Fact]
    public void GenerateAccessToken_WithValidUser_HasCorrectExpiry()
    {
        // Arrange
        var user = CreateTestUser();
        var beforeGeneration = DateTime.UtcNow;

        // Act
        var token = _sut.GenerateAccessToken(user);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        // Assert
        var expectedExpiry = beforeGeneration.AddMinutes(15);
        jwt.ValidTo.Should().BeCloseTo(expectedExpiry, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void GenerateAccessToken_WithMissingSecret_ThrowsInvalidOperationException()
    {
        // Arrange
        var configWithoutSecret = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();
        var service = new JwtService(configWithoutSecret);
        var user = CreateTestUser();

        // Act
        var act = () => service.GenerateAccessToken(user);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("JWT Secret not configured");
    }

    #endregion

    #region GenerateRefreshToken Tests

    [Fact]
    public void GenerateRefreshToken_ReturnsNonEmptyString()
    {
        // Act
        var token = _sut.GenerateRefreshToken();

        // Assert
        token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GenerateRefreshToken_ReturnsUniqueTokens()
    {
        // Act
        var token1 = _sut.GenerateRefreshToken();
        var token2 = _sut.GenerateRefreshToken();

        // Assert
        token1.Should().NotBe(token2);
    }

    [Fact]
    public void GenerateRefreshToken_ReturnsBase64EncodedString()
    {
        // Act
        var token = _sut.GenerateRefreshToken();

        // Assert - should be valid base64
        var act = () => Convert.FromBase64String(token);
        act.Should().NotThrow();
    }

    [Fact]
    public void GenerateRefreshToken_Returns64ByteEncodedToken()
    {
        // Act
        var token = _sut.GenerateRefreshToken();
        var bytes = Convert.FromBase64String(token);

        // Assert
        bytes.Length.Should().Be(64);
    }

    #endregion

    #region ValidateToken Tests

    [Fact]
    public void ValidateToken_WithValidToken_ReturnsClaimsPrincipal()
    {
        // Arrange
        var user = CreateTestUser();
        var token = _sut.GenerateAccessToken(user);

        // Act
        var principal = _sut.ValidateToken(token);

        // Assert
        principal.Should().NotBeNull();
    }

    [Fact]
    public void ValidateToken_WithValidToken_ContainsCorrectSubjectClaim()
    {
        // Arrange
        var user = CreateTestUser();
        var token = _sut.GenerateAccessToken(user);

        // Act
        var principal = _sut.ValidateToken(token);

        // Assert
        principal!.FindFirst(JwtRegisteredClaimNames.Sub)?.Value.Should().Be(user.Id.ToString());
    }

    [Fact]
    public void ValidateToken_WithInvalidSignature_ReturnsNull()
    {
        // Arrange - create token with different secret
        var differentConfig = MockConfiguration.CreateWithJwtSettings(secret: "DifferentSecretKeyThatIsAtLeast32Characters!");
        var differentService = new JwtService(differentConfig);
        var user = CreateTestUser();
        var token = differentService.GenerateAccessToken(user);

        // Act
        var principal = _sut.ValidateToken(token);

        // Assert
        principal.Should().BeNull();
    }

    [Fact]
    public void ValidateToken_WithMalformedToken_ReturnsNull()
    {
        // Arrange
        var malformedToken = "not.a.valid.jwt.token";

        // Act
        var principal = _sut.ValidateToken(malformedToken);

        // Assert
        principal.Should().BeNull();
    }

    [Fact]
    public void ValidateToken_WithEmptyToken_ReturnsNull()
    {
        // Act
        var principal = _sut.ValidateToken("");

        // Assert
        principal.Should().BeNull();
    }

    [Fact]
    public void ValidateToken_WithWrongIssuer_ReturnsNull()
    {
        // Arrange
        var differentIssuerConfig = MockConfiguration.CreateWithJwtSettings(issuer: "different-issuer");
        var differentService = new JwtService(differentIssuerConfig);
        var user = CreateTestUser();
        var token = differentService.GenerateAccessToken(user);

        // Act
        var principal = _sut.ValidateToken(token);

        // Assert
        principal.Should().BeNull();
    }

    [Fact]
    public void ValidateToken_WithExpiredToken_ReturnsNull()
    {
        // Arrange - create service with very short expiry
        var shortExpiryConfig = MockConfiguration.CreateWithJwtSettings(accessTokenMinutes: -1); // Expired immediately
        var shortExpiryService = new JwtService(shortExpiryConfig);
        var user = CreateTestUser();
        var token = shortExpiryService.GenerateAccessToken(user);

        // Act
        var principal = _sut.ValidateToken(token);

        // Assert
        principal.Should().BeNull();
    }

    #endregion

    #region Helper Methods

    private static User CreateTestUser(
        Guid? id = null,
        Guid? tenantId = null,
        string email = "test@example.com",
        string name = "Test User",
        UserRole role = UserRole.Member)
    {
        return new User
        {
            Id = id ?? Guid.NewGuid(),
            TenantId = tenantId ?? Guid.NewGuid(),
            Email = email,
            Name = name,
            Role = role,
            IsActive = true
        };
    }

    #endregion
}
