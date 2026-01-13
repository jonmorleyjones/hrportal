using System.Net;
using System.Net.Http.Json;

namespace Portal.Api.Tests.Endpoints;

public class BillingEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public BillingEndpointsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    #region Subscription Tests

    [Fact]
    public async Task GetSubscription_WithAuthentication_Returns200()
    {
        // Arrange
        var user = await _factory.CreateTestUserAsync(
            email: $"subscription-{Guid.NewGuid()}@example.com",
            password: "TestPassword123!");
        var token = await _factory.GetAuthTokenAsync(user.Email, "TestPassword123!");
        using var client = _factory.CreateAuthenticatedClient(token);

        // Act
        var response = await client.GetAsync("/api/billing/subscription");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<SubscriptionDto>();
        result.Should().NotBeNull();
        result!.Tier.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetSubscription_WithoutAuthentication_Returns401()
    {
        // Arrange
        using var client = _factory.CreateClientWithTenant();

        // Act
        var response = await client.GetAsync("/api/billing/subscription");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetSubscription_ReturnsFeatures()
    {
        // Arrange
        var user = await _factory.CreateTestUserAsync(
            email: $"subscription-features-{Guid.NewGuid()}@example.com",
            password: "TestPassword123!");
        var token = await _factory.GetAuthTokenAsync(user.Email, "TestPassword123!");
        using var client = _factory.CreateAuthenticatedClient(token);

        // Act
        var response = await client.GetAsync("/api/billing/subscription");
        var result = await response.Content.ReadFromJsonAsync<SubscriptionDto>();

        // Assert
        result!.Features.Should().NotBeEmpty();
    }

    #endregion

    #region Invoices Tests

    [Fact]
    public async Task GetInvoices_WithAuthentication_Returns200()
    {
        // Arrange
        var user = await _factory.CreateTestUserAsync(
            email: $"invoices-{Guid.NewGuid()}@example.com",
            password: "TestPassword123!");
        var token = await _factory.GetAuthTokenAsync(user.Email, "TestPassword123!");
        using var client = _factory.CreateAuthenticatedClient(token);

        // Act
        var response = await client.GetAsync("/api/billing/invoices?page=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetInvoices_WithoutAuthentication_Returns401()
    {
        // Arrange
        using var client = _factory.CreateClientWithTenant();

        // Act
        var response = await client.GetAsync("/api/billing/invoices?page=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Upgrade Tests

    [Fact]
    public async Task Upgrade_AsAdmin_ValidTier_Returns200()
    {
        // Arrange
        var adminUser = await _factory.CreateTestUserAsync(
            email: $"admin-upgrade-{Guid.NewGuid()}@example.com",
            password: "TestPassword123!",
            role: UserRole.Admin);
        var token = await _factory.GetAuthTokenAsync(adminUser.Email, "TestPassword123!");
        using var client = _factory.CreateAuthenticatedClient(token);

        var upgradeRequest = new { NewTier = "enterprise" };

        // Act
        var response = await client.PostAsJsonAsync("/api/billing/upgrade", upgradeRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Upgrade_AsAdmin_InvalidTier_Returns400()
    {
        // Arrange
        var adminUser = await _factory.CreateTestUserAsync(
            email: $"admin-invalid-{Guid.NewGuid()}@example.com",
            password: "TestPassword123!",
            role: UserRole.Admin);
        var token = await _factory.GetAuthTokenAsync(adminUser.Email, "TestPassword123!");
        using var client = _factory.CreateAuthenticatedClient(token);

        var upgradeRequest = new { NewTier = "invalid-tier" };

        // Act
        var response = await client.PostAsJsonAsync("/api/billing/upgrade", upgradeRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Upgrade_AsNonAdmin_Returns403()
    {
        // Arrange
        var memberUser = await _factory.CreateTestUserAsync(
            email: $"member-upgrade-{Guid.NewGuid()}@example.com",
            password: "TestPassword123!",
            role: UserRole.Member);
        var token = await _factory.GetAuthTokenAsync(memberUser.Email, "TestPassword123!");
        using var client = _factory.CreateAuthenticatedClient(token);

        var upgradeRequest = new { NewTier = "professional" };

        // Act
        var response = await client.PostAsJsonAsync("/api/billing/upgrade", upgradeRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Theory]
    [InlineData("free")]
    [InlineData("starter")]
    [InlineData("professional")]
    [InlineData("enterprise")]
    public async Task Upgrade_AllValidTiers_Return200(string tier)
    {
        // Arrange
        var adminUser = await _factory.CreateTestUserAsync(
            email: $"admin-tier-{tier}-{Guid.NewGuid()}@example.com",
            password: "TestPassword123!",
            role: UserRole.Admin);
        var token = await _factory.GetAuthTokenAsync(adminUser.Email, "TestPassword123!");
        using var client = _factory.CreateAuthenticatedClient(token);

        var upgradeRequest = new { NewTier = tier };

        // Act
        var response = await client.PostAsJsonAsync("/api/billing/upgrade", upgradeRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region DTOs for deserialization

    private record SubscriptionDto(
        string Tier,
        string Status,
        DateTime StartDate,
        DateTime NextBillingDate,
        decimal MonthlyPrice,
        List<string> Features);

    #endregion
}
