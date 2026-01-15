using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.Extensions.DependencyInjection;
using Portal.Api.Endpoints;

namespace Portal.Api.Tests.Endpoints;

public class HrConsultantEndpointsTests : IClassFixture<HrConsultantWebApplicationFactory>
{
    private readonly HrConsultantWebApplicationFactory _factory;

    public HrConsultantEndpointsTests(HrConsultantWebApplicationFactory factory)
    {
        _factory = factory;
    }

    #region Dashboard Stats Tests

    [Fact]
    public async Task GetDashboardStats_WithValidConsultant_ReturnsStats()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedHrClient();

        // Act
        var response = await client.GetAsync("/api/hr/dashboard/stats");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<HrDashboardStatsResponse>();
        result.Should().NotBeNull();
        result!.TotalTenants.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public async Task GetDashboardStats_WithNoAssignments_ReturnsZeroStats()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedHrClientWithNoAssignments();

        // Act
        var response = await client.GetAsync("/api/hr/dashboard/stats");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<HrDashboardStatsResponse>();
        result.Should().NotBeNull();
        result!.TotalTenants.Should().Be(0);
        result.TotalResponses.Should().Be(0);
        result.PendingReview.Should().Be(0);
    }

    [Fact]
    public async Task GetDashboardStats_WithoutAuth_ReturnsUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/hr/dashboard/stats");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Tenant Stats Tests

    [Fact]
    public async Task GetTenantStats_WithValidAccess_ReturnsStats()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedHrClient();
        var tenantId = _factory.TestTenantId;

        // Act
        var response = await client.GetAsync($"/api/hr/tenants/{tenantId}/stats");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<TenantStatsResponse>();
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetTenantStats_WithNoAccess_ReturnsForbidden()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedHrClientWithNoAssignments();
        var tenantId = _factory.TestTenantId;

        // Act
        var response = await client.GetAsync($"/api/hr/tenants/{tenantId}/stats");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetTenantStats_WithNonExistentTenant_ReturnsForbidden()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedHrClient();
        var nonExistentTenantId = Guid.NewGuid();

        // Act
        var response = await client.GetAsync($"/api/hr/tenants/{nonExistentTenantId}/stats");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region Tenant Responses Tests

    [Fact]
    public async Task GetTenantResponses_WithValidAccess_ReturnsResponses()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedHrClient();
        var tenantId = _factory.TestTenantId;

        // Act
        var response = await client.GetAsync($"/api/hr/tenants/{tenantId}/responses");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<HrResponsesListResponse>();
        result.Should().NotBeNull();
        result!.Responses.Should().NotBeNull();
        result.Page.Should().Be(1);
    }

    [Fact]
    public async Task GetTenantResponses_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedHrClient();
        var tenantId = _factory.TestTenantId;

        // Act
        var response = await client.GetAsync($"/api/hr/tenants/{tenantId}/responses?page=1&pageSize=5");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<HrResponsesListResponse>();
        result.Should().NotBeNull();
        result!.PageSize.Should().Be(5);
    }

    [Fact]
    public async Task GetTenantResponses_FilterByComplete_ReturnsFilteredResponses()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedHrClient();
        var tenantId = _factory.TestTenantId;

        // Act
        var response = await client.GetAsync($"/api/hr/tenants/{tenantId}/responses?isComplete=true");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<HrResponsesListResponse>();
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetTenantResponses_WithNoViewPermission_ReturnsForbidden()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedHrClientWithLimitedPermissions();
        var tenantId = _factory.TestTenantId;

        // Act
        var response = await client.GetAsync($"/api/hr/tenants/{tenantId}/responses");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region Request Types Tests

    [Fact]
    public async Task GetTenantRequestTypes_WithValidAccess_ReturnsRequestTypes()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedHrClient();
        var tenantId = _factory.TestTenantId;

        // Act
        var response = await client.GetAsync($"/api/hr/tenants/{tenantId}/request-types");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<List<HrRequestTypeDto>>();
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetTenantRequestTypeDetail_WithValidAccess_ReturnsDetail()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedHrClient();
        var tenantId = _factory.TestTenantId;
        var requestTypeId = await _factory.CreateTestRequestTypeAsync(tenantId);

        // Act
        var response = await client.GetAsync($"/api/hr/tenants/{tenantId}/request-types/{requestTypeId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<HrRequestTypeDetailDto>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(requestTypeId);
    }

    [Fact]
    public async Task GetTenantRequestTypeDetail_WithNonExistentType_ReturnsNotFound()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedHrClient();
        var tenantId = _factory.TestTenantId;
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await client.GetAsync($"/api/hr/tenants/{tenantId}/request-types/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Update Request Type Tests

    [Fact]
    public async Task UpdateRequestType_WithValidAccess_UpdatesSuccessfully()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedHrClient();
        var tenantId = _factory.TestTenantId;
        var requestTypeId = await _factory.CreateTestRequestTypeAsync(tenantId);

        var updateRequest = new HrUpdateRequestTypeRequest(
            "Updated Name",
            "Updated Description",
            "file-text",
            "{\"pages\":[{\"elements\":[]}]}",
            true
        );

        // Act
        var response = await client.PutAsJsonAsync(
            $"/api/hr/tenants/{tenantId}/request-types/{requestTypeId}",
            updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<HrRequestTypeDetailDto>();
        result.Should().NotBeNull();
        result!.Name.Should().Be("Updated Name");
        result.Description.Should().Be("Updated Description");
    }

    [Fact]
    public async Task UpdateRequestType_WithFormJsonChange_CreatesNewVersion()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedHrClient();
        var tenantId = _factory.TestTenantId;
        var requestTypeId = await _factory.CreateTestRequestTypeAsync(tenantId);

        // Get current version
        var getResponse = await client.GetAsync($"/api/hr/tenants/{tenantId}/request-types/{requestTypeId}");
        var original = await getResponse.Content.ReadFromJsonAsync<HrRequestTypeDetailDto>();
        var originalVersion = original!.CurrentVersionNumber;

        var updateRequest = new HrUpdateRequestTypeRequest(
            "Test Name",
            "Test Description",
            "file-text",
            "{\"pages\":[{\"elements\":[{\"type\":\"text\",\"name\":\"newField\"}]}]}",
            true
        );

        // Act
        var response = await client.PutAsJsonAsync(
            $"/api/hr/tenants/{tenantId}/request-types/{requestTypeId}",
            updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<HrRequestTypeDetailDto>();
        result!.CurrentVersionNumber.Should().BeGreaterThan(originalVersion);
    }

    [Fact]
    public async Task UpdateRequestType_WithNoManagePermission_ReturnsForbidden()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedHrClientWithLimitedPermissions();
        var tenantId = _factory.TestTenantId;
        var requestTypeId = await _factory.CreateTestRequestTypeAsync(tenantId);

        var updateRequest = new HrUpdateRequestTypeRequest(
            "Updated Name",
            "Updated Description",
            "file-text",
            "{}",
            true
        );

        // Act
        var response = await client.PutAsJsonAsync(
            $"/api/hr/tenants/{tenantId}/request-types/{requestTypeId}",
            updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateRequestType_WithNonExistentType_ReturnsNotFound()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedHrClient();
        var tenantId = _factory.TestTenantId;
        var nonExistentId = Guid.NewGuid();

        var updateRequest = new HrUpdateRequestTypeRequest(
            "Updated Name",
            "Updated Description",
            "file-text",
            "{}",
            true
        );

        // Act
        var response = await client.PutAsJsonAsync(
            $"/api/hr/tenants/{tenantId}/request-types/{nonExistentId}",
            updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Response Detail Tests

    [Fact]
    public async Task GetResponseDetail_WithValidAccess_ReturnsDetail()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedHrClient();
        var tenantId = _factory.TestTenantId;
        var responseId = await _factory.CreateTestResponseAsync(tenantId);

        // Act
        var response = await client.GetAsync($"/api/hr/tenants/{tenantId}/responses/{responseId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<HrResponseDetailDto>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(responseId);
    }

    [Fact]
    public async Task GetResponseDetail_WithNonExistentResponse_ReturnsNotFound()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedHrClient();
        var tenantId = _factory.TestTenantId;
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await client.GetAsync($"/api/hr/tenants/{tenantId}/responses/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetResponseDetail_WithNoViewPermission_ReturnsForbidden()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedHrClientWithLimitedPermissions();
        var tenantId = _factory.TestTenantId;
        var responseId = await _factory.CreateTestResponseAsync(tenantId);

        // Act
        var response = await client.GetAsync($"/api/hr/tenants/{tenantId}/responses/{responseId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion
}

public class HrConsultantWebApplicationFactory : CustomWebApplicationFactory
{
    public Guid TestConsultantId { get; } = Guid.NewGuid();
    public Guid NoAssignmentConsultantId { get; } = Guid.NewGuid();
    public Guid LimitedPermissionConsultantId { get; } = Guid.NewGuid();

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);

        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Create HR Consultants
        db.HrConsultants.Add(new HrConsultant
        {
            Id = TestConsultantId,
            Email = "consultant@example.com",
            Name = "Test Consultant",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("TestPassword123!", 4),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });

        db.HrConsultants.Add(new HrConsultant
        {
            Id = NoAssignmentConsultantId,
            Email = "no-assignment@example.com",
            Name = "No Assignment Consultant",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("TestPassword123!", 4),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });

        db.HrConsultants.Add(new HrConsultant
        {
            Id = LimitedPermissionConsultantId,
            Email = "limited@example.com",
            Name = "Limited Permission Consultant",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("TestPassword123!", 4),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });

        // Create tenant assignment for main consultant with full permissions
        db.HrConsultantTenantAssignments.Add(new HrConsultantTenantAssignment
        {
            Id = Guid.NewGuid(),
            HrConsultantId = TestConsultantId,
            TenantId = TestTenantId,
            IsActive = true,
            CanManageRequestTypes = true,
            CanManageSettings = true,
            CanManageBranding = true,
            CanViewResponses = true,
            AssignedAt = DateTime.UtcNow
        });

        // Create tenant assignment for limited permission consultant
        db.HrConsultantTenantAssignments.Add(new HrConsultantTenantAssignment
        {
            Id = Guid.NewGuid(),
            HrConsultantId = LimitedPermissionConsultantId,
            TenantId = TestTenantId,
            IsActive = true,
            CanManageRequestTypes = false,
            CanManageSettings = false,
            CanManageBranding = false,
            CanViewResponses = false,
            AssignedAt = DateTime.UtcNow
        });

        db.SaveChanges();

        return host;
    }

    public HttpClient CreateAuthenticatedHrClient()
    {
        return CreateAuthenticatedHrClientInternal(TestConsultantId);
    }

    public HttpClient CreateAuthenticatedHrClientWithNoAssignments()
    {
        return CreateAuthenticatedHrClientInternal(NoAssignmentConsultantId);
    }

    public HttpClient CreateAuthenticatedHrClientWithLimitedPermissions()
    {
        return CreateAuthenticatedHrClientInternal(LimitedPermissionConsultantId);
    }

    private HttpClient CreateAuthenticatedHrClientInternal(Guid consultantId)
    {
        var client = CreateClient();

        // Generate a test JWT token
        using var scope = Services.CreateScope();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        var jwtService = new JwtService(config);
        var token = jwtService.GenerateAccessToken(consultantId, "consultant@example.com", "HrConsultant");

        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        return client;
    }

    public async Task<Guid> CreateTestRequestTypeAsync(Guid tenantId)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var requestType = new RequestType
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = "Test Request Type",
            Description = "Test Description",
            Icon = "file-text",
            CurrentVersionNumber = 1,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var version = new RequestTypeVersion
        {
            Id = Guid.NewGuid(),
            RequestTypeId = requestType.Id,
            VersionNumber = 1,
            FormJson = "{\"pages\":[{\"elements\":[]}]}",
            CreatedAt = DateTime.UtcNow
        };

        requestType.ActiveVersionId = version.Id;

        db.RequestTypes.Add(requestType);
        db.RequestTypeVersions.Add(version);
        await db.SaveChangesAsync();

        return requestType.Id;
    }

    public async Task<Guid> CreateTestResponseAsync(Guid tenantId)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Create user for the response
        var user = new User
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Email = "response-user@example.com",
            Name = "Response User",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("TestPassword123!", 4),
            Role = UserRole.Member,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        db.Users.Add(user);

        // Create request type and version
        var requestType = new RequestType
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = "Test Request Type",
            Description = "Test Description",
            Icon = "file-text",
            CurrentVersionNumber = 1,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var version = new RequestTypeVersion
        {
            Id = Guid.NewGuid(),
            RequestTypeId = requestType.Id,
            VersionNumber = 1,
            FormJson = "{\"pages\":[{\"elements\":[]}]}",
            CreatedAt = DateTime.UtcNow
        };

        requestType.ActiveVersionId = version.Id;

        db.RequestTypes.Add(requestType);
        db.RequestTypeVersions.Add(version);

        // Create response
        var response = new RequestResponse
        {
            Id = Guid.NewGuid(),
            RequestTypeVersionId = version.Id,
            UserId = user.Id,
            ResponseJson = "{}",
            IsComplete = false,
            StartedAt = DateTime.UtcNow
        };

        db.RequestResponses.Add(response);
        await db.SaveChangesAsync();

        return response.Id;
    }
}
