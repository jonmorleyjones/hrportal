using System.Net;
using System.Net.Http.Json;

namespace Portal.Api.Tests.Endpoints;

public class AuthEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public AuthEndpointsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClientWithTenant();
    }

    #region Login Tests

    [Fact]
    public async Task Login_WithValidCredentials_Returns200WithTokens()
    {
        // Arrange
        await _factory.CreateTestUserAsync(email: "login-test@example.com", password: "TestPassword123!");
        var request = new { Email = "login-test@example.com", Password = "TestPassword123!" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
        result.Should().NotBeNull();
        result!.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
        result.User.Should().NotBeNull();
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsUserInfo()
    {
        // Arrange
        await _factory.CreateTestUserAsync(
            email: "login-userinfo@example.com",
            password: "TestPassword123!",
            name: "Test User Name");
        var request = new { Email = "login-userinfo@example.com", Password = "TestPassword123!" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);
        var result = await response.Content.ReadFromJsonAsync<LoginResponseDto>();

        // Assert
        result!.User.Email.Should().Be("login-userinfo@example.com");
        result.User.Name.Should().Be("Test User Name");
    }

    [Fact]
    public async Task Login_WithInvalidEmail_Returns401()
    {
        // Arrange
        var request = new { Email = "nonexistent@example.com", Password = "TestPassword123!" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_Returns401()
    {
        // Arrange
        await _factory.CreateTestUserAsync(email: "wrong-pass@example.com", password: "CorrectPassword");
        var request = new { Email = "wrong-pass@example.com", Password = "WrongPassword" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithMissingTenantHeader_Returns400()
    {
        // Arrange
        var clientWithoutTenant = _factory.CreateClient(); // No tenant header
        var request = new { Email = "test@example.com", Password = "TestPassword123!" };

        // Act
        var response = await clientWithoutTenant.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Refresh Token Tests

    [Fact]
    public async Task Refresh_WithValidToken_Returns200WithNewTokens()
    {
        // Arrange
        await _factory.CreateTestUserAsync(email: "refresh-test@example.com", password: "TestPassword123!");
        var loginRequest = new { Email = "refresh-test@example.com", Password = "TestPassword123!" };
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>();

        var refreshRequest = new { RefreshToken = loginResult!.RefreshToken };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/refresh", refreshRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<RefreshResponseDto>();
        result.Should().NotBeNull();
        result!.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Refresh_WithInvalidToken_Returns401()
    {
        // Arrange
        var request = new { RefreshToken = "invalid-refresh-token" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/refresh", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Refresh_WithSameTokenTwice_SecondCallReturns401()
    {
        // Arrange
        await _factory.CreateTestUserAsync(email: "refresh-twice@example.com", password: "TestPassword123!");
        var loginRequest = new { Email = "refresh-twice@example.com", Password = "TestPassword123!" };
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>();

        var refreshRequest = new { RefreshToken = loginResult!.RefreshToken };

        // First refresh
        await _client.PostAsJsonAsync("/api/auth/refresh", refreshRequest);

        // Act - second refresh with same token
        var response = await _client.PostAsJsonAsync("/api/auth/refresh", refreshRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Logout Tests

    [Fact]
    public async Task Logout_WithValidToken_Returns200()
    {
        // Arrange
        await _factory.CreateTestUserAsync(email: "logout-test@example.com", password: "TestPassword123!");
        var loginRequest = new { Email = "logout-test@example.com", Password = "TestPassword123!" };
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>();

        var logoutRequest = new { RefreshToken = loginResult!.RefreshToken };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/logout", logoutRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Logout_InvalidatesRefreshToken()
    {
        // Arrange
        await _factory.CreateTestUserAsync(email: "logout-invalidate@example.com", password: "TestPassword123!");
        var loginRequest = new { Email = "logout-invalidate@example.com", Password = "TestPassword123!" };
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>();

        var logoutRequest = new { RefreshToken = loginResult!.RefreshToken };
        await _client.PostAsJsonAsync("/api/auth/logout", logoutRequest);

        // Act - try to use the logged out token
        var refreshRequest = new { RefreshToken = loginResult.RefreshToken };
        var response = await _client.PostAsJsonAsync("/api/auth/refresh", refreshRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Logout_WithInvalidToken_StillReturns200()
    {
        // Arrange
        var request = new { RefreshToken = "nonexistent-token" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/logout", request);

        // Assert - logout is idempotent
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region Forgot Password Tests

    [Fact]
    public async Task ForgotPassword_WithAnyEmail_Returns200()
    {
        // Arrange
        var request = new { Email = "any@example.com" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/forgot-password", request);

        // Assert - always returns success to prevent email enumeration
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region Reset Password Tests

    [Fact]
    public async Task ResetPassword_Returns200()
    {
        // Arrange
        var request = new { Token = "some-token", NewPassword = "NewPassword123!" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/reset-password", request);

        // Assert - TODO endpoint always returns success currently
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region DTOs for deserialization

    private record LoginResponseDto(string AccessToken, string RefreshToken, UserDto User);
    private record RefreshResponseDto(string AccessToken, string RefreshToken);
    private record UserDto(Guid Id, string Email, string Name, UserRole Role, DateTime? LastLoginAt, bool IsActive);

    #endregion
}
