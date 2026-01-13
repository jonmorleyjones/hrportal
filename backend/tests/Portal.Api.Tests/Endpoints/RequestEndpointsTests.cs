using System.Net;
using System.Net.Http.Json;

namespace Portal.Api.Tests.Endpoints;

public class RequestEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public RequestEndpointsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    #region Get Request Types Tests

    [Fact]
    public async Task GetRequestTypes_WithAuthentication_Returns200()
    {
        // Arrange
        var user = await _factory.CreateTestUserAsync(
            email: $"request-types-{Guid.NewGuid()}@example.com",
            password: "TestPassword123!");
        var token = await _factory.GetAuthTokenAsync(user.Email, "TestPassword123!");
        using var client = _factory.CreateAuthenticatedClient(token);

        // Act
        var response = await client.GetAsync("/api/requests/types");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetRequestTypes_WithoutAuthentication_Returns401()
    {
        // Arrange
        using var client = _factory.CreateClientWithTenant();

        // Act
        var response = await client.GetAsync("/api/requests/types");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Admin Create Request Type Tests

    [Fact]
    public async Task CreateRequestType_AsAdmin_Returns201()
    {
        // Arrange
        var adminUser = await _factory.CreateTestUserAsync(
            email: $"admin-create-{Guid.NewGuid()}@example.com",
            password: "TestPassword123!",
            role: UserRole.Admin);
        var token = await _factory.GetAuthTokenAsync(adminUser.Email, "TestPassword123!");
        using var client = _factory.CreateAuthenticatedClient(token);

        var createRequest = new
        {
            Name = "Test Request Type",
            Description = "A test request type",
            Icon = "clipboard-list",
            FormJson = "{\"title\": \"Test Form\", \"pages\": []}"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/requests/admin/types", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<RequestTypeDto>();
        result.Should().NotBeNull();
        result!.Name.Should().Be("Test Request Type");
        result.Description.Should().Be("A test request type");
    }

    [Fact]
    public async Task CreateRequestType_AsNonAdmin_Returns403()
    {
        // Arrange
        var memberUser = await _factory.CreateTestUserAsync(
            email: $"member-create-{Guid.NewGuid()}@example.com",
            password: "TestPassword123!",
            role: UserRole.Member);
        var token = await _factory.GetAuthTokenAsync(memberUser.Email, "TestPassword123!");
        using var client = _factory.CreateAuthenticatedClient(token);

        var createRequest = new
        {
            Name = "Test Request Type",
            FormJson = "{\"title\": \"Test Form\", \"pages\": []}"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/requests/admin/types", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region Get Request Type Tests

    [Fact]
    public async Task GetRequestType_NonExistent_Returns404()
    {
        // Arrange
        var user = await _factory.CreateTestUserAsync(
            email: $"get-type-{Guid.NewGuid()}@example.com",
            password: "TestPassword123!");
        var token = await _factory.GetAuthTokenAsync(user.Email, "TestPassword123!");
        using var client = _factory.CreateAuthenticatedClient(token);
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await client.GetAsync($"/api/requests/types/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Submit Response Tests

    [Fact]
    public async Task SubmitResponse_NonExistentType_Returns404()
    {
        // Arrange
        var user = await _factory.CreateTestUserAsync(
            email: $"submit-none-{Guid.NewGuid()}@example.com",
            password: "TestPassword123!");
        var token = await _factory.GetAuthTokenAsync(user.Email, "TestPassword123!");
        using var client = _factory.CreateAuthenticatedClient(token);

        var submitRequest = new { ResponseJson = "{}", IsComplete = true };
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await client.PostAsJsonAsync($"/api/requests/types/{nonExistentId}/responses", submitRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task SubmitResponse_WithoutAuthentication_Returns401()
    {
        // Arrange
        using var client = _factory.CreateClientWithTenant();
        var submitRequest = new { ResponseJson = "{}", IsComplete = true };

        // Act
        var response = await client.PostAsJsonAsync($"/api/requests/types/{Guid.NewGuid()}/responses", submitRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Admin Get Request Types Tests

    [Fact]
    public async Task AdminGetRequestTypes_AsAdmin_Returns200()
    {
        // Arrange
        var adminUser = await _factory.CreateTestUserAsync(
            email: $"admin-get-types-{Guid.NewGuid()}@example.com",
            password: "TestPassword123!",
            role: UserRole.Admin);
        var token = await _factory.GetAuthTokenAsync(adminUser.Email, "TestPassword123!");
        using var client = _factory.CreateAuthenticatedClient(token);

        // Act
        var response = await client.GetAsync("/api/requests/admin/types");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task AdminGetRequestTypes_AsNonAdmin_Returns403()
    {
        // Arrange
        var memberUser = await _factory.CreateTestUserAsync(
            email: $"member-get-types-{Guid.NewGuid()}@example.com",
            password: "TestPassword123!",
            role: UserRole.Member);
        var token = await _factory.GetAuthTokenAsync(memberUser.Email, "TestPassword123!");
        using var client = _factory.CreateAuthenticatedClient(token);

        // Act
        var response = await client.GetAsync("/api/requests/admin/types");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region Admin Update Request Type Tests

    [Fact]
    public async Task UpdateRequestType_NonExistent_Returns404()
    {
        // Arrange
        var adminUser = await _factory.CreateTestUserAsync(
            email: $"admin-update-none-{Guid.NewGuid()}@example.com",
            password: "TestPassword123!",
            role: UserRole.Admin);
        var token = await _factory.GetAuthTokenAsync(adminUser.Email, "TestPassword123!");
        using var client = _factory.CreateAuthenticatedClient(token);

        var updateRequest = new
        {
            Name = "Updated Name",
            FormJson = "{}",
            IsActive = true
        };

        // Act
        var response = await client.PutAsJsonAsync($"/api/requests/admin/types/{Guid.NewGuid()}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Admin Delete Request Type Tests

    [Fact]
    public async Task DeleteRequestType_NonExistent_Returns404()
    {
        // Arrange
        var adminUser = await _factory.CreateTestUserAsync(
            email: $"admin-delete-none-{Guid.NewGuid()}@example.com",
            password: "TestPassword123!",
            role: UserRole.Admin);
        var token = await _factory.GetAuthTokenAsync(adminUser.Email, "TestPassword123!");
        using var client = _factory.CreateAuthenticatedClient(token);

        // Act
        var response = await client.DeleteAsync($"/api/requests/admin/types/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Admin Get All Responses Tests

    [Fact]
    public async Task AdminGetAllResponses_AsAdmin_Returns200()
    {
        // Arrange
        var adminUser = await _factory.CreateTestUserAsync(
            email: $"admin-responses-{Guid.NewGuid()}@example.com",
            password: "TestPassword123!",
            role: UserRole.Admin);
        var token = await _factory.GetAuthTokenAsync(adminUser.Email, "TestPassword123!");
        using var client = _factory.CreateAuthenticatedClient(token);

        // Act
        var response = await client.GetAsync("/api/requests/admin/responses");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task AdminGetAllResponses_AsNonAdmin_Returns403()
    {
        // Arrange
        var memberUser = await _factory.CreateTestUserAsync(
            email: $"member-responses-{Guid.NewGuid()}@example.com",
            password: "TestPassword123!",
            role: UserRole.Member);
        var token = await _factory.GetAuthTokenAsync(memberUser.Email, "TestPassword123!");
        using var client = _factory.CreateAuthenticatedClient(token);

        // Act
        var response = await client.GetAsync("/api/requests/admin/responses");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region DTOs for deserialization

    private record RequestTypeDto(
        Guid Id,
        string Name,
        string? Description,
        string Icon,
        int CurrentVersionNumber,
        string FormJson,
        bool IsActive,
        DateTime CreatedAt,
        DateTime UpdatedAt);

    #endregion
}
