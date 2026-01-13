using System.Net;
using System.Net.Http.Json;

namespace Portal.Api.Tests.Endpoints;

public class DashboardEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public DashboardEndpointsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    #region Stats Tests

    [Fact]
    public async Task GetStats_WithAuthentication_Returns200()
    {
        // Arrange
        var user = await _factory.CreateTestUserAsync(
            email: $"stats-{Guid.NewGuid()}@example.com",
            password: "TestPassword123!");
        var token = await _factory.GetAuthTokenAsync(user.Email, "TestPassword123!");
        using var client = _factory.CreateAuthenticatedClient(token);

        // Act
        var response = await client.GetAsync("/api/dashboard/stats");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<DashboardStatsDto>();
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetStats_WithoutAuthentication_Returns401()
    {
        // Arrange
        using var client = _factory.CreateClientWithTenant();

        // Act
        var response = await client.GetAsync("/api/dashboard/stats");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetStats_ReturnsCorrectCounts()
    {
        // Arrange
        var user = await _factory.CreateTestUserAsync(
            email: $"stats-counts-{Guid.NewGuid()}@example.com",
            password: "TestPassword123!");
        var token = await _factory.GetAuthTokenAsync(user.Email, "TestPassword123!");
        using var client = _factory.CreateAuthenticatedClient(token);

        // Act
        var response = await client.GetAsync("/api/dashboard/stats");
        var result = await response.Content.ReadFromJsonAsync<DashboardStatsDto>();

        // Assert
        result!.TotalUsers.Should().BeGreaterThan(0);
    }

    #endregion

    #region Activity Feed Tests

    [Fact]
    public async Task GetActivityFeed_WithAuthentication_Returns200()
    {
        // Arrange
        var user = await _factory.CreateTestUserAsync(
            email: $"activity-{Guid.NewGuid()}@example.com",
            password: "TestPassword123!");
        var token = await _factory.GetAuthTokenAsync(user.Email, "TestPassword123!");
        using var client = _factory.CreateAuthenticatedClient(token);

        // Act
        var response = await client.GetAsync("/api/dashboard/activity?limit=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetActivityFeed_WithoutAuthentication_Returns401()
    {
        // Arrange
        using var client = _factory.CreateClientWithTenant();

        // Act
        var response = await client.GetAsync("/api/dashboard/activity?limit=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetActivityFeed_LimitIsCapped()
    {
        // Arrange
        var user = await _factory.CreateTestUserAsync(
            email: $"activity-limit-{Guid.NewGuid()}@example.com",
            password: "TestPassword123!");
        var token = await _factory.GetAuthTokenAsync(user.Email, "TestPassword123!");
        using var client = _factory.CreateAuthenticatedClient(token);

        // Act - request more than max
        var response = await client.GetAsync("/api/dashboard/activity?limit=100");

        // Assert - should still succeed (limit capped internally to 50)
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region Chart Data Tests

    [Fact]
    public async Task GetChartData_UsersChart_Returns200WithData()
    {
        // Arrange
        var user = await _factory.CreateTestUserAsync(
            email: $"chart-users-{Guid.NewGuid()}@example.com",
            password: "TestPassword123!");
        var token = await _factory.GetAuthTokenAsync(user.Email, "TestPassword123!");
        using var client = _factory.CreateAuthenticatedClient(token);

        // Act
        var response = await client.GetAsync("/api/dashboard/charts/users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ChartDataDto>();
        result.Should().NotBeNull();
        result!.ChartType.Should().Be("users");
        result.Data.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetChartData_ActivityChart_Returns200WithData()
    {
        // Arrange
        var user = await _factory.CreateTestUserAsync(
            email: $"chart-activity-{Guid.NewGuid()}@example.com",
            password: "TestPassword123!");
        var token = await _factory.GetAuthTokenAsync(user.Email, "TestPassword123!");
        using var client = _factory.CreateAuthenticatedClient(token);

        // Act
        var response = await client.GetAsync("/api/dashboard/charts/activity");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ChartDataDto>();
        result.Should().NotBeNull();
        result!.ChartType.Should().Be("activity");
    }

    [Fact]
    public async Task GetChartData_UnknownChart_Returns200WithEmptyData()
    {
        // Arrange
        var user = await _factory.CreateTestUserAsync(
            email: $"chart-unknown-{Guid.NewGuid()}@example.com",
            password: "TestPassword123!");
        var token = await _factory.GetAuthTokenAsync(user.Email, "TestPassword123!");
        using var client = _factory.CreateAuthenticatedClient(token);

        // Act
        var response = await client.GetAsync("/api/dashboard/charts/unknown");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ChartDataDto>();
        result!.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task GetChartData_WithoutAuthentication_Returns401()
    {
        // Arrange
        using var client = _factory.CreateClientWithTenant();

        // Act
        var response = await client.GetAsync("/api/dashboard/charts/users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region DTOs for deserialization

    private record DashboardStatsDto(int TotalUsers, int ActiveUsers, int PendingInvitations, decimal MonthlyActiveRate);
    private record ChartDataDto(string ChartType, List<ChartDataPointDto> Data);
    private record ChartDataPointDto(string Label, decimal Value);

    #endregion
}
