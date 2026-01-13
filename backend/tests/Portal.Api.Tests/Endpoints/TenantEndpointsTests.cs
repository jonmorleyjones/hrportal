using System.Net;
using System.Net.Http.Json;

namespace Portal.Api.Tests.Endpoints;

public class TenantEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public TenantEndpointsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    #region Resolve Tenant Tests

    [Fact]
    public async Task ResolveTenant_WithValidSlug_Returns200WithTenantDto()
    {
        // Arrange
        using var client = _factory.CreateClient(); // No tenant header needed

        // Act
        var response = await client.GetAsync($"/api/tenants/resolve?slug={_factory.TestTenantSlug}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<TenantDto>();
        result.Should().NotBeNull();
        result!.Slug.Should().Be(_factory.TestTenantSlug);
    }

    [Fact]
    public async Task ResolveTenant_WithInvalidSlug_Returns404()
    {
        // Arrange
        using var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/tenants/resolve?slug=nonexistent-tenant");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ResolveTenant_DoesNotRequireAuthentication()
    {
        // Arrange - no auth token
        using var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync($"/api/tenants/resolve?slug={_factory.TestTenantSlug}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region Get Current Tenant Tests

    [Fact]
    public async Task GetCurrentTenant_WithAuthentication_Returns200()
    {
        // Arrange
        var user = await _factory.CreateTestUserAsync(
            email: $"tenant-current-{Guid.NewGuid()}@example.com",
            password: "TestPassword123!");
        var token = await _factory.GetAuthTokenAsync(user.Email, "TestPassword123!");
        using var client = _factory.CreateAuthenticatedClient(token);

        // Act
        var response = await client.GetAsync("/api/tenants/current");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<TenantDto>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(_factory.TestTenantId);
    }

    [Fact]
    public async Task GetCurrentTenant_WithoutAuthentication_Returns401()
    {
        // Arrange
        using var client = _factory.CreateClientWithTenant();

        // Act
        var response = await client.GetAsync("/api/tenants/current");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Update Settings Tests

    [Fact]
    public async Task UpdateSettings_AsAdmin_Returns200()
    {
        // Arrange
        var adminUser = await _factory.CreateTestUserAsync(
            email: $"admin-settings-{Guid.NewGuid()}@example.com",
            password: "TestPassword123!",
            role: UserRole.Admin);
        var token = await _factory.GetAuthTokenAsync(adminUser.Email, "TestPassword123!");
        using var client = _factory.CreateAuthenticatedClient(token);

        var settingsRequest = new
        {
            Settings = new
            {
                EnableNotifications = true,
                Timezone = "America/New_York",
                Language = "en"
            }
        };

        // Act
        var response = await client.PutAsJsonAsync("/api/tenants/settings", settingsRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UpdateSettings_AsNonAdmin_Returns403()
    {
        // Arrange
        var memberUser = await _factory.CreateTestUserAsync(
            email: $"member-settings-{Guid.NewGuid()}@example.com",
            password: "TestPassword123!",
            role: UserRole.Member);
        var token = await _factory.GetAuthTokenAsync(memberUser.Email, "TestPassword123!");
        using var client = _factory.CreateAuthenticatedClient(token);

        var settingsRequest = new { Settings = new { EnableNotifications = true } };

        // Act
        var response = await client.PutAsJsonAsync("/api/tenants/settings", settingsRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region Update Branding Tests

    [Fact]
    public async Task UpdateBranding_AsAdmin_Returns200()
    {
        // Arrange
        var adminUser = await _factory.CreateTestUserAsync(
            email: $"admin-branding-{Guid.NewGuid()}@example.com",
            password: "TestPassword123!",
            role: UserRole.Admin);
        var token = await _factory.GetAuthTokenAsync(adminUser.Email, "TestPassword123!");
        using var client = _factory.CreateAuthenticatedClient(token);

        var brandingRequest = new
        {
            Branding = new
            {
                LogoUrl = "https://example.com/logo.png",
                PrimaryColor = "#ff0000",
                SecondaryColor = "#00ff00"
            }
        };

        // Act
        var response = await client.PutAsJsonAsync("/api/tenants/branding", brandingRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UpdateBranding_AsNonAdmin_Returns403()
    {
        // Arrange
        var memberUser = await _factory.CreateTestUserAsync(
            email: $"member-branding-{Guid.NewGuid()}@example.com",
            password: "TestPassword123!",
            role: UserRole.Member);
        var token = await _factory.GetAuthTokenAsync(memberUser.Email, "TestPassword123!");
        using var client = _factory.CreateAuthenticatedClient(token);

        var brandingRequest = new { Branding = new { PrimaryColor = "#ff0000" } };

        // Act
        var response = await client.PutAsJsonAsync("/api/tenants/branding", brandingRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region DTOs for deserialization

    private record TenantDto(Guid Id, string Slug, string Name, string SubscriptionTier, object? Settings, object? Branding);

    #endregion
}
