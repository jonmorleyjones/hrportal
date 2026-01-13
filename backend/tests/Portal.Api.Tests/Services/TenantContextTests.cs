namespace Portal.Api.Tests.Services;

public class TenantContextTests
{
    [Fact]
    public void TenantId_BeforeSetTenant_ReturnsDefaultGuid()
    {
        // Arrange
        var sut = new TenantContext();

        // Act
        var result = sut.TenantId;

        // Assert
        result.Should().Be(Guid.Empty);
    }

    [Fact]
    public void TenantSlug_BeforeSetTenant_ReturnsNull()
    {
        // Arrange
        var sut = new TenantContext();

        // Act
        var result = sut.TenantSlug;

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void SetTenant_WithValidValues_SetsTenantIdCorrectly()
    {
        // Arrange
        var sut = new TenantContext();
        var tenantId = Guid.NewGuid();
        var slug = "test-tenant";

        // Act
        sut.SetTenant(tenantId, slug);

        // Assert
        sut.TenantId.Should().Be(tenantId);
    }

    [Fact]
    public void SetTenant_WithValidValues_SetsTenantSlugCorrectly()
    {
        // Arrange
        var sut = new TenantContext();
        var tenantId = Guid.NewGuid();
        var slug = "test-tenant";

        // Act
        sut.SetTenant(tenantId, slug);

        // Assert
        sut.TenantSlug.Should().Be(slug);
    }

    [Fact]
    public void SetTenant_WithNullSlug_SetsSlugToNull()
    {
        // Arrange
        var sut = new TenantContext();
        var tenantId = Guid.NewGuid();

        // Act
        sut.SetTenant(tenantId, null);

        // Assert
        sut.TenantId.Should().Be(tenantId);
        sut.TenantSlug.Should().BeNull();
    }

    [Fact]
    public void SetTenant_CalledMultipleTimes_UpdatesValues()
    {
        // Arrange
        var sut = new TenantContext();
        var firstTenantId = Guid.NewGuid();
        var secondTenantId = Guid.NewGuid();

        // Act
        sut.SetTenant(firstTenantId, "first-tenant");
        sut.SetTenant(secondTenantId, "second-tenant");

        // Assert
        sut.TenantId.Should().Be(secondTenantId);
        sut.TenantSlug.Should().Be("second-tenant");
    }
}
