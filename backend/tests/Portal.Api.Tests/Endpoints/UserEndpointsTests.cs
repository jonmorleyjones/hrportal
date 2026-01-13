using System.Net;
using System.Net.Http.Json;

namespace Portal.Api.Tests.Endpoints;

public class UserEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public UserEndpointsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    #region List Users Tests

    [Fact]
    public async Task ListUsers_WithAuthentication_Returns200()
    {
        // Arrange
        var user = await _factory.CreateTestUserAsync(
            email: $"list-users-{Guid.NewGuid()}@example.com",
            password: "TestPassword123!");
        var token = await _factory.GetAuthTokenAsync(user.Email, "TestPassword123!");
        using var client = _factory.CreateAuthenticatedClient(token);

        // Act
        var response = await client.GetAsync("/api/users?page=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<UserListResponseDto>();
        result.Should().NotBeNull();
        result!.Users.Should().NotBeNull();
    }

    [Fact]
    public async Task ListUsers_WithoutAuthentication_Returns401()
    {
        // Arrange
        using var client = _factory.CreateClientWithTenant();

        // Act
        var response = await client.GetAsync("/api/users?page=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ListUsers_ReturnsPaginatedResults()
    {
        // Arrange
        var user = await _factory.CreateTestUserAsync(
            email: $"paginate-{Guid.NewGuid()}@example.com",
            password: "TestPassword123!");
        var token = await _factory.GetAuthTokenAsync(user.Email, "TestPassword123!");
        using var client = _factory.CreateAuthenticatedClient(token);

        // Act
        var response = await client.GetAsync("/api/users?page=1&pageSize=5");
        var result = await response.Content.ReadFromJsonAsync<UserListResponseDto>();

        // Assert
        result!.Page.Should().Be(1);
        result.PageSize.Should().Be(5);
        result.TotalCount.Should().BeGreaterThan(0);
    }

    #endregion

    #region Get User Tests

    [Fact]
    public async Task GetUser_ExistingUser_Returns200WithUserDto()
    {
        // Arrange
        var targetUser = await _factory.CreateTestUserAsync(
            email: $"get-user-{Guid.NewGuid()}@example.com",
            password: "TestPassword123!",
            name: "Target User");
        var token = await _factory.GetAuthTokenAsync(targetUser.Email, "TestPassword123!");
        using var client = _factory.CreateAuthenticatedClient(token);

        // Act
        var response = await client.GetAsync($"/api/users/{targetUser.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<UserDto>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(targetUser.Id);
        result.Email.Should().Be(targetUser.Email);
    }

    [Fact]
    public async Task GetUser_NonExistentUser_Returns404()
    {
        // Arrange
        var user = await _factory.CreateTestUserAsync(
            email: $"get-notfound-{Guid.NewGuid()}@example.com",
            password: "TestPassword123!");
        var token = await _factory.GetAuthTokenAsync(user.Email, "TestPassword123!");
        using var client = _factory.CreateAuthenticatedClient(token);
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await client.GetAsync($"/api/users/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetUser_WithoutAuthentication_Returns401()
    {
        // Arrange
        using var client = _factory.CreateClientWithTenant();

        // Act
        var response = await client.GetAsync($"/api/users/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Invite User Tests

    [Fact]
    public async Task InviteUser_AsAdmin_Returns201()
    {
        // Arrange
        var adminUser = await _factory.CreateTestUserAsync(
            email: $"admin-invite-{Guid.NewGuid()}@example.com",
            password: "TestPassword123!",
            role: UserRole.Admin);
        var token = await _factory.GetAuthTokenAsync(adminUser.Email, "TestPassword123!");
        using var client = _factory.CreateAuthenticatedClient(token);

        var inviteRequest = new { Email = $"invited-{Guid.NewGuid()}@example.com", Role = UserRole.Member };

        // Act
        var response = await client.PostAsJsonAsync("/api/users/invite", inviteRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task InviteUser_AsNonAdmin_Returns403()
    {
        // Arrange
        var memberUser = await _factory.CreateTestUserAsync(
            email: $"member-invite-{Guid.NewGuid()}@example.com",
            password: "TestPassword123!",
            role: UserRole.Member);
        var token = await _factory.GetAuthTokenAsync(memberUser.Email, "TestPassword123!");
        using var client = _factory.CreateAuthenticatedClient(token);

        var inviteRequest = new { Email = "newuser@example.com", Role = UserRole.Member };

        // Act
        var response = await client.PostAsJsonAsync("/api/users/invite", inviteRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task InviteUser_ExistingEmail_Returns400()
    {
        // Arrange
        var existingUser = await _factory.CreateTestUserAsync(
            email: $"existing-{Guid.NewGuid()}@example.com",
            password: "TestPassword123!");
        var adminUser = await _factory.CreateTestUserAsync(
            email: $"admin-exist-{Guid.NewGuid()}@example.com",
            password: "TestPassword123!",
            role: UserRole.Admin);
        var token = await _factory.GetAuthTokenAsync(adminUser.Email, "TestPassword123!");
        using var client = _factory.CreateAuthenticatedClient(token);

        var inviteRequest = new { Email = existingUser.Email, Role = UserRole.Member };

        // Act
        var response = await client.PostAsJsonAsync("/api/users/invite", inviteRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Update User Role Tests

    [Fact]
    public async Task UpdateUserRole_AsAdmin_Returns200()
    {
        // Arrange
        var targetUser = await _factory.CreateTestUserAsync(
            email: $"target-role-{Guid.NewGuid()}@example.com",
            password: "TestPassword123!",
            role: UserRole.Member);
        var adminUser = await _factory.CreateTestUserAsync(
            email: $"admin-role-{Guid.NewGuid()}@example.com",
            password: "TestPassword123!",
            role: UserRole.Admin);
        var token = await _factory.GetAuthTokenAsync(adminUser.Email, "TestPassword123!");
        using var client = _factory.CreateAuthenticatedClient(token);

        var updateRequest = new { Role = UserRole.Viewer };

        // Act
        var response = await client.PutAsJsonAsync($"/api/users/{targetUser.Id}/role", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UpdateUserRole_SelfDemotion_Returns400()
    {
        // Arrange
        var adminUser = await _factory.CreateTestUserAsync(
            email: $"admin-selfdemo-{Guid.NewGuid()}@example.com",
            password: "TestPassword123!",
            role: UserRole.Admin);
        var token = await _factory.GetAuthTokenAsync(adminUser.Email, "TestPassword123!");
        using var client = _factory.CreateAuthenticatedClient(token);

        var updateRequest = new { Role = UserRole.Member };

        // Act
        var response = await client.PutAsJsonAsync($"/api/users/{adminUser.Id}/role", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateUserRole_NonExistentUser_Returns404()
    {
        // Arrange
        var adminUser = await _factory.CreateTestUserAsync(
            email: $"admin-notfound-{Guid.NewGuid()}@example.com",
            password: "TestPassword123!",
            role: UserRole.Admin);
        var token = await _factory.GetAuthTokenAsync(adminUser.Email, "TestPassword123!");
        using var client = _factory.CreateAuthenticatedClient(token);

        var updateRequest = new { Role = UserRole.Member };

        // Act
        var response = await client.PutAsJsonAsync($"/api/users/{Guid.NewGuid()}/role", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Deactivate User Tests

    [Fact]
    public async Task DeactivateUser_AsAdmin_Returns200()
    {
        // Arrange
        var targetUser = await _factory.CreateTestUserAsync(
            email: $"target-deact-{Guid.NewGuid()}@example.com",
            password: "TestPassword123!");
        var adminUser = await _factory.CreateTestUserAsync(
            email: $"admin-deact-{Guid.NewGuid()}@example.com",
            password: "TestPassword123!",
            role: UserRole.Admin);
        var token = await _factory.GetAuthTokenAsync(adminUser.Email, "TestPassword123!");
        using var client = _factory.CreateAuthenticatedClient(token);

        // Act
        var response = await client.DeleteAsync($"/api/users/{targetUser.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DeactivateUser_SelfDeletion_Returns400()
    {
        // Arrange
        var adminUser = await _factory.CreateTestUserAsync(
            email: $"admin-selfdelete-{Guid.NewGuid()}@example.com",
            password: "TestPassword123!",
            role: UserRole.Admin);
        var token = await _factory.GetAuthTokenAsync(adminUser.Email, "TestPassword123!");
        using var client = _factory.CreateAuthenticatedClient(token);

        // Act
        var response = await client.DeleteAsync($"/api/users/{adminUser.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeactivateUser_AsNonAdmin_Returns403()
    {
        // Arrange
        var targetUser = await _factory.CreateTestUserAsync(
            email: $"target-nonadmin-{Guid.NewGuid()}@example.com",
            password: "TestPassword123!");
        var memberUser = await _factory.CreateTestUserAsync(
            email: $"member-deact-{Guid.NewGuid()}@example.com",
            password: "TestPassword123!",
            role: UserRole.Member);
        var token = await _factory.GetAuthTokenAsync(memberUser.Email, "TestPassword123!");
        using var client = _factory.CreateAuthenticatedClient(token);

        // Act
        var response = await client.DeleteAsync($"/api/users/{targetUser.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region DTOs for deserialization

    private record UserListResponseDto(List<UserDto> Users, int TotalCount, int Page, int PageSize);
    private record UserDto(Guid Id, string Email, string Name, UserRole Role, DateTime? LastLoginAt, bool IsActive);

    #endregion
}
