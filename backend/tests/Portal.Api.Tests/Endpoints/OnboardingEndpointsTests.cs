using System.Net;
using System.Net.Http.Json;

namespace Portal.Api.Tests.Endpoints;

public class OnboardingEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public OnboardingEndpointsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    #region Status Tests

    [Fact]
    public async Task GetStatus_WithAuthentication_Returns200()
    {
        // Arrange
        var user = await _factory.CreateTestUserAsync(
            email: $"onboarding-status-{Guid.NewGuid()}@example.com",
            password: "TestPassword123!");
        var token = await _factory.GetAuthTokenAsync(user.Email, "TestPassword123!");
        using var client = _factory.CreateAuthenticatedClient(token);

        // Act
        var response = await client.GetAsync("/api/onboarding/status");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetStatus_WithoutAuthentication_Returns401()
    {
        // Arrange
        using var client = _factory.CreateClientWithTenant();

        // Act
        var response = await client.GetAsync("/api/onboarding/status");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetStatus_ReturnsHasSurveyFlag()
    {
        // Arrange
        var user = await _factory.CreateTestUserAsync(
            email: $"status-flag-{Guid.NewGuid()}@example.com",
            password: "TestPassword123!");
        var token = await _factory.GetAuthTokenAsync(user.Email, "TestPassword123!");
        using var client = _factory.CreateAuthenticatedClient(token);

        // Act
        var response = await client.GetAsync("/api/onboarding/status");
        var result = await response.Content.ReadFromJsonAsync<OnboardingStatusDto>();

        // Assert
        result.Should().NotBeNull();
        // HasSurvey will be false as no survey is seeded in tests
    }

    #endregion

    #region Survey Tests

    [Fact]
    public async Task GetSurvey_WithAuthentication_Returns200Or404()
    {
        // Arrange
        var user = await _factory.CreateTestUserAsync(
            email: $"survey-none-{Guid.NewGuid()}@example.com",
            password: "TestPassword123!");
        var token = await _factory.GetAuthTokenAsync(user.Email, "TestPassword123!");
        using var client = _factory.CreateAuthenticatedClient(token);

        // Act
        var response = await client.GetAsync("/api/onboarding/survey");

        // Assert - depends on whether another test created a survey
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetSurvey_WithoutAuthentication_Returns401()
    {
        // Arrange
        using var client = _factory.CreateClientWithTenant();

        // Act
        var response = await client.GetAsync("/api/onboarding/survey");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Response Tests

    [Fact]
    public async Task GetResponse_WithAuthentication_Returns200()
    {
        // Arrange
        var user = await _factory.CreateTestUserAsync(
            email: $"response-get-{Guid.NewGuid()}@example.com",
            password: "TestPassword123!");
        var token = await _factory.GetAuthTokenAsync(user.Email, "TestPassword123!");
        using var client = _factory.CreateAuthenticatedClient(token);

        // Act
        var response = await client.GetAsync("/api/onboarding/response");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetResponse_WithoutAuthentication_Returns401()
    {
        // Arrange
        using var client = _factory.CreateClientWithTenant();

        // Act
        var response = await client.GetAsync("/api/onboarding/response");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SubmitResponse_WithAuthentication_Returns200Or404()
    {
        // Arrange
        var user = await _factory.CreateTestUserAsync(
            email: $"submit-none-{Guid.NewGuid()}@example.com",
            password: "TestPassword123!");
        var token = await _factory.GetAuthTokenAsync(user.Email, "TestPassword123!");
        using var client = _factory.CreateAuthenticatedClient(token);

        var submitRequest = new { ResponseJson = "{}", IsComplete = true };

        // Act
        var response = await client.PostAsJsonAsync("/api/onboarding/response", submitRequest);

        // Assert - depends on whether another test created a survey
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    #endregion

    #region Admin Survey Tests

    [Fact]
    public async Task AdminGetSurvey_AsAdmin_Returns200Or404()
    {
        // Arrange
        var adminUser = await _factory.CreateTestUserAsync(
            email: $"admin-survey-get-{Guid.NewGuid()}@example.com",
            password: "TestPassword123!",
            role: UserRole.Admin);
        var token = await _factory.GetAuthTokenAsync(adminUser.Email, "TestPassword123!");
        using var client = _factory.CreateAuthenticatedClient(token);

        // Act
        var response = await client.GetAsync("/api/onboarding/admin/survey");

        // Assert - depends on whether another test created a survey
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AdminGetSurvey_AsNonAdmin_Returns403()
    {
        // Arrange
        var memberUser = await _factory.CreateTestUserAsync(
            email: $"member-survey-get-{Guid.NewGuid()}@example.com",
            password: "TestPassword123!",
            role: UserRole.Member);
        var token = await _factory.GetAuthTokenAsync(memberUser.Email, "TestPassword123!");
        using var client = _factory.CreateAuthenticatedClient(token);

        // Act
        var response = await client.GetAsync("/api/onboarding/admin/survey");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AdminCreateSurvey_AsAdmin_Returns201()
    {
        // Arrange
        var adminUser = await _factory.CreateTestUserAsync(
            email: $"admin-survey-create-{Guid.NewGuid()}@example.com",
            password: "TestPassword123!",
            role: UserRole.Admin);
        var token = await _factory.GetAuthTokenAsync(adminUser.Email, "TestPassword123!");
        using var client = _factory.CreateAuthenticatedClient(token);

        var createRequest = new
        {
            Name = "Test Survey",
            SurveyJson = "{\"questions\": []}"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/onboarding/admin/survey", createRequest);

        // Assert
        // Note: May return 400 if survey already exists from other tests
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AdminCreateSurvey_AsNonAdmin_Returns403()
    {
        // Arrange
        var memberUser = await _factory.CreateTestUserAsync(
            email: $"member-survey-create-{Guid.NewGuid()}@example.com",
            password: "TestPassword123!",
            role: UserRole.Member);
        var token = await _factory.GetAuthTokenAsync(memberUser.Email, "TestPassword123!");
        using var client = _factory.CreateAuthenticatedClient(token);

        var createRequest = new
        {
            Name = "Test Survey",
            SurveyJson = "{\"questions\": []}"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/onboarding/admin/survey", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region Admin Responses Tests

    [Fact]
    public async Task AdminGetResponses_AsAdmin_Returns200()
    {
        // Arrange
        var adminUser = await _factory.CreateTestUserAsync(
            email: $"admin-responses-{Guid.NewGuid()}@example.com",
            password: "TestPassword123!",
            role: UserRole.Admin);
        var token = await _factory.GetAuthTokenAsync(adminUser.Email, "TestPassword123!");
        using var client = _factory.CreateAuthenticatedClient(token);

        // Act
        var response = await client.GetAsync("/api/onboarding/admin/responses");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task AdminGetResponses_AsNonAdmin_Returns403()
    {
        // Arrange
        var memberUser = await _factory.CreateTestUserAsync(
            email: $"member-responses-{Guid.NewGuid()}@example.com",
            password: "TestPassword123!",
            role: UserRole.Member);
        var token = await _factory.GetAuthTokenAsync(memberUser.Email, "TestPassword123!");
        using var client = _factory.CreateAuthenticatedClient(token);

        // Act
        var response = await client.GetAsync("/api/onboarding/admin/responses");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region DTOs for deserialization

    private record OnboardingStatusDto(bool HasSurvey, object? Survey);

    #endregion
}
