using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Portal.Api.Tests.Helpers;

namespace Portal.Api.Tests.Endpoints;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private const string TestJwtSecret = "TestSecretKeyThatIsAtLeast32CharactersLong!";

    public string TestTenantSlug { get; } = "test-tenant";
    public Guid TestTenantId { get; } = Guid.NewGuid();
    private readonly string _databaseName = Guid.NewGuid().ToString();

    public CustomWebApplicationFactory()
    {
        // Set environment variables BEFORE the host is built
        Environment.SetEnvironmentVariable("Jwt__Secret", TestJwtSecret);
        Environment.SetEnvironmentVariable("Jwt__Issuer", "portal-api");
        Environment.SetEnvironmentVariable("Jwt__AccessTokenExpiryMinutes", "15");
        Environment.SetEnvironmentVariable("Jwt__RefreshTokenExpiryDays", "7");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                {"Jwt:Secret", TestJwtSecret},
                {"Jwt:Issuer", "portal-api"},
                {"Jwt:AccessTokenExpiryMinutes", "15"},
                {"Jwt:RefreshTokenExpiryDays", "7"},
                {"ConnectionStrings:DefaultConnection", ""}
            });
        });

        builder.ConfigureServices(services =>
        {
            // Remove existing DbContext registrations
            services.RemoveAll(typeof(DbContextOptions<AppDbContext>));
            services.RemoveAll(typeof(AppDbContext));

            // Add in-memory database
            services.AddDbContext<AppDbContext>((sp, options) =>
            {
                options.UseInMemoryDatabase(_databaseName);
            });
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);

        // Seed test data after host is created
        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tenantContext = scope.ServiceProvider.GetRequiredService<ITenantContext>();

        tenantContext.SetTenant(TestTenantId, TestTenantSlug);

        // Check if tenant already exists
        if (!db.Tenants.Any(t => t.Id == TestTenantId))
        {
            db.Tenants.Add(new Tenant
            {
                Id = TestTenantId,
                Slug = TestTenantSlug,
                Name = "Test Tenant",
                IsActive = true,
                SubscriptionTier = "professional",
                CreatedAt = DateTime.UtcNow
            });
            db.SaveChanges();
        }

        return host;
    }

    public HttpClient CreateClientWithTenant()
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Add("X-Tenant-ID", TestTenantSlug);
        return client;
    }

    public async Task<User> CreateTestUserAsync(
        string email = "test@example.com",
        string password = "TestPassword123!",
        string name = "Test User",
        UserRole role = UserRole.Member)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tenantContext = scope.ServiceProvider.GetRequiredService<ITenantContext>();

        tenantContext.SetTenant(TestTenantId, TestTenantSlug);

        var user = new User
        {
            Id = Guid.NewGuid(),
            TenantId = TestTenantId,
            Email = email,
            Name = name,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password, 4),
            Role = role,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        return user;
    }

    public async Task<string> GetAuthTokenAsync(string email, string password)
    {
        using var client = CreateClientWithTenant();
        var loginRequest = new { Email = email, Password = password };
        var response = await client.PostAsJsonAsync("/api/auth/login", loginRequest);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Login failed with status {response.StatusCode}: {errorContent}");
        }

        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        return result?.AccessToken ?? throw new InvalidOperationException("No access token in response");
    }

    public HttpClient CreateAuthenticatedClient(string token)
    {
        var client = CreateClientWithTenant();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            // Clean up environment variables
            Environment.SetEnvironmentVariable("Jwt__Secret", null);
            Environment.SetEnvironmentVariable("Jwt__Issuer", null);
            Environment.SetEnvironmentVariable("Jwt__AccessTokenExpiryMinutes", null);
            Environment.SetEnvironmentVariable("Jwt__RefreshTokenExpiryDays", null);
        }
        base.Dispose(disposing);
    }

    private record LoginResponse(string AccessToken, string RefreshToken, object User);
}
