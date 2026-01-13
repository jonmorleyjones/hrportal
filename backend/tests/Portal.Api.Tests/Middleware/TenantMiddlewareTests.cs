using Microsoft.AspNetCore.Http;
using Portal.Api.Middleware;
using Portal.Api.Tests.Helpers;

namespace Portal.Api.Tests.Middleware;

public class TenantMiddlewareTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly ITenantContext _tenantContext;
    private readonly Guid _tenantId;
    private readonly TenantMiddleware _sut;
    private bool _nextCalled;

    public TenantMiddlewareTests()
    {
        (_context, _tenantContext, _tenantId) = TestDatabaseFactory.CreateWithDefaultTenant();
        _sut = new TenantMiddleware(_ =>
        {
            _nextCalled = true;
            return Task.CompletedTask;
        });
        _nextCalled = false;
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    #region Excluded Paths Tests

    [Theory]
    [InlineData("/api/tenants/resolve")]
    [InlineData("/api/tenants/resolve?slug=test")]
    [InlineData("/swagger")]
    [InlineData("/swagger/index.html")]
    [InlineData("/health")]
    public async Task InvokeAsync_WithExcludedPath_SkipsTenantResolutionAndCallsNext(string path)
    {
        // Arrange
        var httpContext = CreateHttpContext(path: path);

        // Act
        await _sut.InvokeAsync(httpContext, _context, _tenantContext);

        // Assert
        _nextCalled.Should().BeTrue();
    }

    [Theory]
    [InlineData("/api/tenants/resolve")]
    [InlineData("/swagger")]
    [InlineData("/health")]
    public async Task InvokeAsync_WithExcludedPath_DoesNotRequireTenantHeader(string path)
    {
        // Arrange - no tenant header set
        var httpContext = CreateHttpContext(path: path, tenantSlug: null);

        // Act
        await _sut.InvokeAsync(httpContext, _context, _tenantContext);

        // Assert
        httpContext.Response.StatusCode.Should().Be(200);
        _nextCalled.Should().BeTrue();
    }

    #endregion

    #region Valid Tenant Tests

    [Fact]
    public async Task InvokeAsync_WithValidTenantHeader_SetsTenantContext()
    {
        // Arrange
        var httpContext = CreateHttpContext(path: "/api/users", tenantSlug: "test-tenant");

        // Act
        await _sut.InvokeAsync(httpContext, _context, _tenantContext);

        // Assert
        _tenantContext.TenantId.Should().Be(_tenantId);
        _tenantContext.TenantSlug.Should().Be("test-tenant");
    }

    [Fact]
    public async Task InvokeAsync_WithValidTenantHeader_CallsNext()
    {
        // Arrange
        var httpContext = CreateHttpContext(path: "/api/users", tenantSlug: "test-tenant");

        // Act
        await _sut.InvokeAsync(httpContext, _context, _tenantContext);

        // Assert
        _nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_WithValidTenantHeader_Returns200()
    {
        // Arrange
        var httpContext = CreateHttpContext(path: "/api/users", tenantSlug: "test-tenant");

        // Act
        await _sut.InvokeAsync(httpContext, _context, _tenantContext);

        // Assert
        httpContext.Response.StatusCode.Should().Be(200);
    }

    #endregion

    #region Missing Tenant Tests

    [Fact]
    public async Task InvokeAsync_WithMissingTenantHeader_Returns400()
    {
        // Arrange
        var httpContext = CreateHttpContext(path: "/api/users", tenantSlug: null, host: "localhost");

        // Act
        await _sut.InvokeAsync(httpContext, _context, _tenantContext);

        // Assert
        httpContext.Response.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task InvokeAsync_WithMissingTenantHeader_DoesNotCallNext()
    {
        // Arrange
        var httpContext = CreateHttpContext(path: "/api/users", tenantSlug: null, host: "localhost");

        // Act
        await _sut.InvokeAsync(httpContext, _context, _tenantContext);

        // Assert
        _nextCalled.Should().BeFalse();
    }

    [Fact]
    public async Task InvokeAsync_WithEmptyTenantHeader_Returns400()
    {
        // Arrange
        var httpContext = CreateHttpContext(path: "/api/users", tenantSlug: "", host: "localhost");

        // Act
        await _sut.InvokeAsync(httpContext, _context, _tenantContext);

        // Assert
        httpContext.Response.StatusCode.Should().Be(400);
    }

    #endregion

    #region Invalid Tenant Tests

    [Fact]
    public async Task InvokeAsync_WithInvalidTenantSlug_Returns404()
    {
        // Arrange
        var httpContext = CreateHttpContext(path: "/api/users", tenantSlug: "nonexistent-tenant");

        // Act
        await _sut.InvokeAsync(httpContext, _context, _tenantContext);

        // Assert
        httpContext.Response.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task InvokeAsync_WithInvalidTenantSlug_DoesNotCallNext()
    {
        // Arrange
        var httpContext = CreateHttpContext(path: "/api/users", tenantSlug: "nonexistent-tenant");

        // Act
        await _sut.InvokeAsync(httpContext, _context, _tenantContext);

        // Assert
        _nextCalled.Should().BeFalse();
    }

    [Fact]
    public async Task InvokeAsync_WithInactiveTenant_Returns404()
    {
        // Arrange
        var inactiveTenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Slug = "inactive-tenant",
            Name = "Inactive Tenant",
            IsActive = false
        };
        _context.Tenants.Add(inactiveTenant);
        await _context.SaveChangesAsync();

        var httpContext = CreateHttpContext(path: "/api/users", tenantSlug: "inactive-tenant");

        // Act
        await _sut.InvokeAsync(httpContext, _context, _tenantContext);

        // Assert
        httpContext.Response.StatusCode.Should().Be(404);
    }

    #endregion

    #region Subdomain Fallback Tests

    [Fact]
    public async Task InvokeAsync_WithSubdomain_ExtractsTenantSlugFromHost()
    {
        // Arrange - Reset tenant context to new one to track if it gets set
        var newTenantContext = new TenantContext();
        var httpContext = CreateHttpContext(
            path: "/api/users",
            tenantSlug: null,
            host: "test-tenant.example.com"
        );

        // Act
        await _sut.InvokeAsync(httpContext, _context, newTenantContext);

        // Assert
        newTenantContext.TenantSlug.Should().Be("test-tenant");
        newTenantContext.TenantId.Should().Be(_tenantId);
    }

    [Fact]
    public async Task InvokeAsync_WithHeaderAndSubdomain_PrefersHeader()
    {
        // Arrange - Create a second tenant for subdomain
        var otherTenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Slug = "other-tenant",
            Name = "Other Tenant",
            IsActive = true
        };
        _context.Tenants.Add(otherTenant);
        await _context.SaveChangesAsync();

        var newTenantContext = new TenantContext();
        var httpContext = CreateHttpContext(
            path: "/api/users",
            tenantSlug: "test-tenant", // Header value
            host: "other-tenant.example.com" // Subdomain value
        );

        // Act
        await _sut.InvokeAsync(httpContext, _context, newTenantContext);

        // Assert - Should use header value
        newTenantContext.TenantSlug.Should().Be("test-tenant");
        newTenantContext.TenantId.Should().Be(_tenantId);
    }

    #endregion

    #region Case Sensitivity Tests

    [Fact]
    public async Task InvokeAsync_WithUppercasePath_StillExcludesCorrectly()
    {
        // Arrange
        var httpContext = CreateHttpContext(path: "/SWAGGER/index.html", tenantSlug: null);

        // Act
        await _sut.InvokeAsync(httpContext, _context, _tenantContext);

        // Assert
        _nextCalled.Should().BeTrue();
    }

    #endregion

    #region Helper Methods

    private static DefaultHttpContext CreateHttpContext(
        string path = "/api/test",
        string? tenantSlug = "test-tenant",
        string host = "localhost")
    {
        var context = new DefaultHttpContext();
        context.Request.Path = path;
        context.Request.Host = new HostString(host);
        context.Response.Body = new MemoryStream();

        if (tenantSlug != null)
        {
            context.Request.Headers["X-Tenant-ID"] = tenantSlug;
        }

        return context;
    }

    #endregion
}
