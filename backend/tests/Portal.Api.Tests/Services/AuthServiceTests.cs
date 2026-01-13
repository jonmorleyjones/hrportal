using Portal.Api.Tests.Helpers;

namespace Portal.Api.Tests.Services;

public class AuthServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly ITenantContext _tenantContext;
    private readonly Guid _tenantId;
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly IConfiguration _configuration;
    private readonly AuthService _sut;
    private readonly TestDataBuilder _dataBuilder;

    public AuthServiceTests()
    {
        (_context, _tenantContext, _tenantId) = TestDatabaseFactory.CreateWithDefaultTenant();
        _jwtServiceMock = new Mock<IJwtService>();
        _configuration = MockConfiguration.CreateWithJwtSettings();
        _sut = new AuthService(_context, _jwtServiceMock.Object, _configuration);
        _dataBuilder = new TestDataBuilder(_context, _tenantId);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    #region LoginAsync Tests

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsLoginResponse()
    {
        // Arrange
        var password = "TestPassword123!";
        var user = _dataBuilder.CreateUser(email: "user@test.com", password: password, name: "Test User");
        _dataBuilder.Save();

        _jwtServiceMock.Setup(x => x.GenerateAccessToken(It.IsAny<User>()))
            .Returns("mock-access-token");
        _jwtServiceMock.Setup(x => x.GenerateRefreshToken())
            .Returns("mock-refresh-token");

        // Act
        var result = await _sut.LoginAsync("user@test.com", password, _tenantId);

        // Assert
        result.Should().NotBeNull();
        result!.AccessToken.Should().Be("mock-access-token");
        result.RefreshToken.Should().Be("mock-refresh-token");
        result.User.Email.Should().Be("user@test.com");
        result.User.Name.Should().Be("Test User");
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_UpdatesLastLoginAt()
    {
        // Arrange
        var password = "TestPassword123!";
        var user = _dataBuilder.CreateUser(email: "user@test.com", password: password);
        _dataBuilder.Save();
        var beforeLogin = DateTime.UtcNow;

        _jwtServiceMock.Setup(x => x.GenerateAccessToken(It.IsAny<User>())).Returns("token");
        _jwtServiceMock.Setup(x => x.GenerateRefreshToken()).Returns("refresh");

        // Act
        await _sut.LoginAsync("user@test.com", password, _tenantId);

        // Assert
        var updatedUser = await _context.Users.FindAsync(user.Id);
        updatedUser!.LastLoginAt.Should().NotBeNull();
        updatedUser.LastLoginAt.Should().BeOnOrAfter(beforeLogin);
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_CreatesRefreshToken()
    {
        // Arrange
        var password = "TestPassword123!";
        var user = _dataBuilder.CreateUser(email: "user@test.com", password: password);
        _dataBuilder.Save();

        _jwtServiceMock.Setup(x => x.GenerateAccessToken(It.IsAny<User>())).Returns("token");
        _jwtServiceMock.Setup(x => x.GenerateRefreshToken()).Returns("test-refresh-token");

        // Act
        await _sut.LoginAsync("user@test.com", password, _tenantId);

        // Assert
        var storedToken = await _context.RefreshTokens.FirstOrDefaultAsync(t => t.Token == "test-refresh-token");
        storedToken.Should().NotBeNull();
        storedToken!.UserId.Should().Be(user.Id);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidEmail_ReturnsNull()
    {
        // Arrange
        var user = _dataBuilder.CreateUser(email: "user@test.com", password: "password");
        _dataBuilder.Save();

        // Act
        var result = await _sut.LoginAsync("nonexistent@test.com", "password", _tenantId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ReturnsNull()
    {
        // Arrange
        var user = _dataBuilder.CreateUser(email: "user@test.com", password: "CorrectPassword");
        _dataBuilder.Save();

        // Act
        var result = await _sut.LoginAsync("user@test.com", "WrongPassword", _tenantId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_WithInactiveUser_ReturnsNull()
    {
        // Arrange
        var user = _dataBuilder.CreateUser(email: "user@test.com", password: "password", isActive: false);
        _dataBuilder.Save();

        // Act
        var result = await _sut.LoginAsync("user@test.com", "password", _tenantId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_WithWrongTenant_ReturnsNull()
    {
        // Arrange
        var user = _dataBuilder.CreateUser(email: "user@test.com", password: "password");
        _dataBuilder.Save();
        var differentTenantId = Guid.NewGuid();

        // Act
        var result = await _sut.LoginAsync("user@test.com", "password", differentTenantId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_WithEmptyEmail_ReturnsNull()
    {
        // Act
        var result = await _sut.LoginAsync("", "password", _tenantId);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region RefreshTokenAsync Tests

    [Fact]
    public async Task RefreshTokenAsync_WithValidToken_ReturnsNewTokens()
    {
        // Arrange
        var user = _dataBuilder.CreateUser(email: "user@test.com");
        _dataBuilder.Save();
        var refreshToken = _dataBuilder.CreateRefreshToken(user.Id, token: "valid-refresh-token");
        _dataBuilder.Save();

        _jwtServiceMock.Setup(x => x.GenerateAccessToken(It.IsAny<User>())).Returns("new-access-token");
        _jwtServiceMock.Setup(x => x.GenerateRefreshToken()).Returns("new-refresh-token");

        // Act
        var result = await _sut.RefreshTokenAsync("valid-refresh-token");

        // Assert
        result.Should().NotBeNull();
        result!.AccessToken.Should().Be("new-access-token");
        result.RefreshToken.Should().Be("new-refresh-token");
    }

    [Fact]
    public async Task RefreshTokenAsync_WithValidToken_RevokesOldToken()
    {
        // Arrange
        var user = _dataBuilder.CreateUser(email: "user@test.com");
        _dataBuilder.Save();
        var refreshToken = _dataBuilder.CreateRefreshToken(user.Id, token: "valid-refresh-token");
        _dataBuilder.Save();

        _jwtServiceMock.Setup(x => x.GenerateAccessToken(It.IsAny<User>())).Returns("token");
        _jwtServiceMock.Setup(x => x.GenerateRefreshToken()).Returns("new-token");

        // Act
        await _sut.RefreshTokenAsync("valid-refresh-token");

        // Assert
        var oldToken = await _context.RefreshTokens.FirstOrDefaultAsync(t => t.Token == "valid-refresh-token");
        oldToken!.RevokedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task RefreshTokenAsync_WithValidToken_CreatesNewRefreshToken()
    {
        // Arrange
        var user = _dataBuilder.CreateUser(email: "user@test.com");
        _dataBuilder.Save();
        var refreshToken = _dataBuilder.CreateRefreshToken(user.Id, token: "valid-refresh-token");
        _dataBuilder.Save();

        _jwtServiceMock.Setup(x => x.GenerateAccessToken(It.IsAny<User>())).Returns("token");
        _jwtServiceMock.Setup(x => x.GenerateRefreshToken()).Returns("new-refresh-token");

        // Act
        await _sut.RefreshTokenAsync("valid-refresh-token");

        // Assert
        var newToken = await _context.RefreshTokens.FirstOrDefaultAsync(t => t.Token == "new-refresh-token");
        newToken.Should().NotBeNull();
        newToken!.UserId.Should().Be(user.Id);
    }

    [Fact]
    public async Task RefreshTokenAsync_WithExpiredToken_ReturnsNull()
    {
        // Arrange
        var user = _dataBuilder.CreateUser(email: "user@test.com");
        _dataBuilder.Save();
        var expiredToken = _dataBuilder.CreateRefreshToken(
            user.Id,
            token: "expired-token",
            expiresAt: DateTime.UtcNow.AddDays(-1)
        );
        _dataBuilder.Save();

        // Act
        var result = await _sut.RefreshTokenAsync("expired-token");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task RefreshTokenAsync_WithRevokedToken_ReturnsNull()
    {
        // Arrange
        var user = _dataBuilder.CreateUser(email: "user@test.com");
        _dataBuilder.Save();
        var revokedToken = _dataBuilder.CreateRefreshToken(
            user.Id,
            token: "revoked-token",
            revokedAt: DateTime.UtcNow.AddHours(-1)
        );
        _dataBuilder.Save();

        // Act
        var result = await _sut.RefreshTokenAsync("revoked-token");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task RefreshTokenAsync_WithInactiveUser_ReturnsNull()
    {
        // Arrange
        var user = _dataBuilder.CreateUser(email: "user@test.com", isActive: false);
        _dataBuilder.Save();
        var refreshToken = _dataBuilder.CreateRefreshToken(user.Id, token: "valid-token");
        _dataBuilder.Save();

        // Act
        var result = await _sut.RefreshTokenAsync("valid-token");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task RefreshTokenAsync_WithNonExistentToken_ReturnsNull()
    {
        // Act
        var result = await _sut.RefreshTokenAsync("nonexistent-token");

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region LogoutAsync Tests

    [Fact]
    public async Task LogoutAsync_WithValidToken_ReturnsTrue()
    {
        // Arrange
        var user = _dataBuilder.CreateUser(email: "user@test.com");
        _dataBuilder.Save();
        var refreshToken = _dataBuilder.CreateRefreshToken(user.Id, token: "logout-token");
        _dataBuilder.Save();

        // Act
        var result = await _sut.LogoutAsync("logout-token");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task LogoutAsync_WithValidToken_RevokesToken()
    {
        // Arrange
        var user = _dataBuilder.CreateUser(email: "user@test.com");
        _dataBuilder.Save();
        var refreshToken = _dataBuilder.CreateRefreshToken(user.Id, token: "logout-token");
        _dataBuilder.Save();

        // Act
        await _sut.LogoutAsync("logout-token");

        // Assert
        var token = await _context.RefreshTokens.FirstOrDefaultAsync(t => t.Token == "logout-token");
        token!.RevokedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task LogoutAsync_WithInvalidToken_ReturnsFalse()
    {
        // Act
        var result = await _sut.LogoutAsync("nonexistent-token");

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region HashPassword Tests

    [Fact]
    public void HashPassword_ReturnsNonEmptyString()
    {
        // Act
        var hash = _sut.HashPassword("password");

        // Assert
        hash.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void HashPassword_ReturnsDifferentHashForSamePassword()
    {
        // Act
        var hash1 = _sut.HashPassword("password");
        var hash2 = _sut.HashPassword("password");

        // Assert - BCrypt should generate different salts each time
        hash1.Should().NotBe(hash2);
    }

    [Fact]
    public void HashPassword_ReturnsValidBCryptHash()
    {
        // Act
        var hash = _sut.HashPassword("password");

        // Assert - BCrypt hashes start with $2
        hash.Should().StartWith("$2");
    }

    #endregion

    #region VerifyPassword Tests

    [Fact]
    public void VerifyPassword_WithCorrectPassword_ReturnsTrue()
    {
        // Arrange
        var password = "TestPassword123!";
        var hash = _sut.HashPassword(password);

        // Act
        var result = _sut.VerifyPassword(password, hash);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_WithWrongPassword_ReturnsFalse()
    {
        // Arrange
        var hash = _sut.HashPassword("CorrectPassword");

        // Act
        var result = _sut.VerifyPassword("WrongPassword", hash);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void VerifyPassword_IsCaseSensitive()
    {
        // Arrange
        var hash = _sut.HashPassword("Password");

        // Act
        var result = _sut.VerifyPassword("password", hash);

        // Assert
        result.Should().BeFalse();
    }

    #endregion
}
