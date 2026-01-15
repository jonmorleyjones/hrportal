using System.Net;
using System.Net.Http.Json;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace Portal.Api.Tests.Endpoints;

public class FileEndpointsTests : IClassFixture<FileTestWebApplicationFactory>
{
    private readonly FileTestWebApplicationFactory _factory;

    public FileEndpointsTests(FileTestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    #region Upload Tests

    [Fact]
    public async Task Upload_WithValidFile_Returns201()
    {
        // Arrange
        var client = await _factory.CreateAuthenticatedClientAsync();
        var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes("test file content"));
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/plain");

        var formData = new MultipartFormDataContent();
        formData.Add(fileContent, "file", "test.txt");

        // Act
        var response = await client.PostAsync("/api/files/upload?questionName=testQuestion", formData);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<FileUploadResponseDto>();
        result.Should().NotBeNull();
        result!.OriginalFileName.Should().Be("test.txt");
        result.ContentType.Should().Be("text/plain");
    }

    [Fact]
    public async Task Upload_WithMissingQuestionName_Returns400()
    {
        // Arrange
        var client = await _factory.CreateAuthenticatedClientAsync();
        var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes("test file content"));
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/plain");

        var formData = new MultipartFormDataContent();
        formData.Add(fileContent, "file", "test.txt");

        // Act
        var response = await client.PostAsync("/api/files/upload", formData);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Upload_WithEmptyQuestionName_Returns400()
    {
        // Arrange
        var client = await _factory.CreateAuthenticatedClientAsync();
        var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes("test file content"));
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/plain");

        var formData = new MultipartFormDataContent();
        formData.Add(fileContent, "file", "test.txt");

        // Act
        var response = await client.PostAsync("/api/files/upload?questionName=", formData);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Upload_WithoutAuth_Returns401()
    {
        // Arrange
        var client = _factory.CreateClientWithTenant();
        var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes("test file content"));
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/plain");

        var formData = new MultipartFormDataContent();
        formData.Add(fileContent, "file", "test.txt");

        // Act
        var response = await client.PostAsync("/api/files/upload?questionName=testQuestion", formData);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Upload_WithDisallowedFileType_Returns400()
    {
        // Arrange
        var client = await _factory.CreateAuthenticatedClientAsync();
        var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes("test file content"));
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/x-executable");

        var formData = new MultipartFormDataContent();
        formData.Add(fileContent, "file", "test.exe");

        // Act
        var response = await client.PostAsync("/api/files/upload?questionName=testQuestion", formData);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Download Tests

    [Fact]
    public async Task Download_OwnFile_ReturnsFile()
    {
        // Arrange
        var client = await _factory.CreateAuthenticatedClientAsync();
        var fileId = await _factory.CreateTestFileAsync();

        // Act
        var response = await client.GetAsync($"/api/files/{fileId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.Should().Be("text/plain");
    }

    [Fact]
    public async Task Download_NonExistentFile_Returns404()
    {
        // Arrange
        var client = await _factory.CreateAuthenticatedClientAsync();
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await client.GetAsync($"/api/files/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Download_OtherUsersFile_AsNonAdmin_Returns403()
    {
        // Arrange
        var fileId = await _factory.CreateTestFileAsync();
        var otherUserClient = await _factory.CreateAuthenticatedClientForOtherUserAsync();

        // Act
        var response = await otherUserClient.GetAsync($"/api/files/{fileId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Download_OtherUsersFile_AsAdmin_ReturnsFile()
    {
        // Arrange
        var fileId = await _factory.CreateTestFileAsync();
        var adminClient = await _factory.CreateAuthenticatedAdminClientAsync();

        // Act
        var response = await adminClient.GetAsync($"/api/files/{fileId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Download_WithoutAuth_Returns401()
    {
        // Arrange
        var client = _factory.CreateClientWithTenant();
        var fileId = await _factory.CreateTestFileAsync();

        // Act
        var response = await client.GetAsync($"/api/files/{fileId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task Delete_OwnUnlinkedFile_Returns204()
    {
        // Arrange
        var client = await _factory.CreateAuthenticatedClientAsync();
        var fileId = await _factory.CreateTestFileAsync();

        // Act
        var response = await client.DeleteAsync($"/api/files/{fileId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify file is deleted
        var getResponse = await client.GetAsync($"/api/files/{fileId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_NonExistentFile_Returns404()
    {
        // Arrange
        var client = await _factory.CreateAuthenticatedClientAsync();
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await client.DeleteAsync($"/api/files/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_OtherUsersFile_AsNonAdmin_Returns403()
    {
        // Arrange
        var fileId = await _factory.CreateTestFileAsync();
        var otherUserClient = await _factory.CreateAuthenticatedClientForOtherUserAsync();

        // Act
        var response = await otherUserClient.DeleteAsync($"/api/files/{fileId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Delete_LinkedFile_AsNonAdmin_Returns400()
    {
        // Arrange
        var client = await _factory.CreateAuthenticatedClientAsync();
        var fileId = await _factory.CreateTestFileAsync(linked: true);

        // Act
        var response = await client.DeleteAsync($"/api/files/{fileId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Delete_LinkedFile_AsAdmin_Returns204()
    {
        // Arrange
        var fileId = await _factory.CreateTestFileAsync(linked: true);
        var adminClient = await _factory.CreateAuthenticatedAdminClientAsync();

        // Act
        var response = await adminClient.DeleteAsync($"/api/files/{fileId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Delete_WithoutAuth_Returns401()
    {
        // Arrange
        var client = _factory.CreateClientWithTenant();
        var fileId = await _factory.CreateTestFileAsync();

        // Act
        var response = await client.DeleteAsync($"/api/files/{fileId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Link Files Tests

    [Fact]
    public async Task LinkFiles_WithValidData_Returns200()
    {
        // Arrange
        var client = await _factory.CreateAuthenticatedClientAsync();
        var fileId = await _factory.CreateTestFileAsync();
        var responseId = await _factory.CreateTestResponseForUserAsync();

        var request = new LinkFilesRequest(responseId, new List<Guid> { fileId });

        // Act
        var response = await client.PostAsJsonAsync("/api/files/link", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task LinkFiles_WithNonExistentResponse_Returns404()
    {
        // Arrange
        var client = await _factory.CreateAuthenticatedClientAsync();
        var fileId = await _factory.CreateTestFileAsync();
        var nonExistentResponseId = Guid.NewGuid();

        var request = new LinkFilesRequest(nonExistentResponseId, new List<Guid> { fileId });

        // Act
        var response = await client.PostAsJsonAsync("/api/files/link", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task LinkFiles_ToOtherUsersResponse_Returns403()
    {
        // Arrange
        var client = await _factory.CreateAuthenticatedClientAsync();
        var fileId = await _factory.CreateTestFileAsync();
        var otherUserResponseId = await _factory.CreateTestResponseForOtherUserAsync();

        var request = new LinkFilesRequest(otherUserResponseId, new List<Guid> { fileId });

        // Act
        var response = await client.PostAsJsonAsync("/api/files/link", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task LinkFiles_WithoutAuth_Returns401()
    {
        // Arrange
        var client = _factory.CreateClientWithTenant();
        var request = new LinkFilesRequest(Guid.NewGuid(), new List<Guid> { Guid.NewGuid() });

        // Act
        var response = await client.PostAsJsonAsync("/api/files/link", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Admin Get Response Files Tests

    [Fact]
    public async Task GetResponseFiles_AsAdmin_ReturnsFiles()
    {
        // Arrange
        var adminClient = await _factory.CreateAuthenticatedAdminClientAsync();
        var responseId = await _factory.CreateTestResponseWithFilesAsync();

        // Act
        var response = await adminClient.GetAsync($"/api/files/admin/response/{responseId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<List<FileInfoDto>>();
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetResponseFiles_AsNonAdmin_Returns403()
    {
        // Arrange
        var client = await _factory.CreateAuthenticatedClientAsync();
        var responseId = await _factory.CreateTestResponseWithFilesAsync();

        // Act
        var response = await client.GetAsync($"/api/files/admin/response/{responseId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion
}

public class FileTestWebApplicationFactory : CustomWebApplicationFactory
{
    private Guid _testUserId = Guid.NewGuid();
    private Guid _otherUserId = Guid.NewGuid();
    private Guid _adminUserId = Guid.NewGuid();
    private readonly string _testFilesPath;

    public FileTestWebApplicationFactory()
    {
        _testFilesPath = Path.Combine(Path.GetTempPath(), "portal-test-files", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testFilesPath);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureServices(services =>
        {
            // Configure file storage to use temp directory
            services.Configure<FileStorageOptions>(options =>
            {
                options.BasePath = _testFilesPath;
                options.MaxFileSizeBytes = 25 * 1024 * 1024;
            });
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);

        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tenantContext = scope.ServiceProvider.GetRequiredService<ITenantContext>();

        tenantContext.SetTenant(TestTenantId, TestTenantSlug);

        // Create test user
        db.Users.Add(new User
        {
            Id = _testUserId,
            TenantId = TestTenantId,
            Email = "filetest@example.com",
            Name = "File Test User",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("TestPassword123!", 4),
            Role = UserRole.Member,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });

        // Create other user
        db.Users.Add(new User
        {
            Id = _otherUserId,
            TenantId = TestTenantId,
            Email = "other@example.com",
            Name = "Other User",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("TestPassword123!", 4),
            Role = UserRole.Member,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });

        // Create admin user
        db.Users.Add(new User
        {
            Id = _adminUserId,
            TenantId = TestTenantId,
            Email = "admin@example.com",
            Name = "Admin User",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("TestPassword123!", 4),
            Role = UserRole.Admin,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });

        db.SaveChanges();

        return host;
    }

    public async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        var token = await GetAuthTokenForUserAsync(_testUserId, "filetest@example.com", "Member");
        return CreateAuthenticatedClient(token);
    }

    public async Task<HttpClient> CreateAuthenticatedClientForOtherUserAsync()
    {
        var token = await GetAuthTokenForUserAsync(_otherUserId, "other@example.com", "Member");
        return CreateAuthenticatedClient(token);
    }

    public async Task<HttpClient> CreateAuthenticatedAdminClientAsync()
    {
        var token = await GetAuthTokenForUserAsync(_adminUserId, "admin@example.com", "Admin");
        return CreateAuthenticatedClient(token);
    }

    private Task<string> GetAuthTokenForUserAsync(Guid userId, string email, string role)
    {
        using var scope = Services.CreateScope();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var jwtService = new JwtService(config);
        var token = jwtService.GenerateAccessToken(userId, email, role);
        return Task.FromResult(token);
    }

    public async Task<Guid> CreateTestFileAsync(bool linked = false)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var fileId = Guid.NewGuid();
        var storedFileName = Guid.NewGuid().ToString();
        var now = DateTime.UtcNow;
        var relativePath = Path.Combine(
            TestTenantId.ToString(),
            now.Year.ToString(),
            now.Month.ToString("D2"),
            storedFileName
        );

        // Create physical file
        var fullPath = Path.Combine(_testFilesPath, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        await File.WriteAllTextAsync(fullPath, "test file content");

        Guid? responseId = null;
        if (linked)
        {
            responseId = await CreateTestResponseForUserInternalAsync(db);
        }

        var file = new UploadedFile
        {
            Id = fileId,
            TenantId = TestTenantId,
            UserId = _testUserId,
            RequestResponseId = responseId,
            QuestionName = "testQuestion",
            OriginalFileName = "test.txt",
            StoredFileName = storedFileName,
            ContentType = "text/plain",
            FileSizeBytes = 17,
            StoragePath = relativePath,
            UploadedAt = now
        };

        db.UploadedFiles.Add(file);
        await db.SaveChangesAsync();

        return fileId;
    }

    public async Task<Guid> CreateTestResponseForUserAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await CreateTestResponseForUserInternalAsync(db);
    }

    private async Task<Guid> CreateTestResponseForUserInternalAsync(AppDbContext db)
    {
        var requestType = new RequestType
        {
            Id = Guid.NewGuid(),
            TenantId = TestTenantId,
            Name = "Test Request Type",
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
            FormJson = "{}",
            CreatedAt = DateTime.UtcNow
        };

        requestType.ActiveVersionId = version.Id;

        db.RequestTypes.Add(requestType);
        db.RequestTypeVersions.Add(version);

        var response = new RequestResponse
        {
            Id = Guid.NewGuid(),
            RequestTypeVersionId = version.Id,
            UserId = _testUserId,
            ResponseJson = "{}",
            IsComplete = false,
            StartedAt = DateTime.UtcNow
        };

        db.RequestResponses.Add(response);
        await db.SaveChangesAsync();

        return response.Id;
    }

    public async Task<Guid> CreateTestResponseForOtherUserAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var requestType = new RequestType
        {
            Id = Guid.NewGuid(),
            TenantId = TestTenantId,
            Name = "Test Request Type",
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
            FormJson = "{}",
            CreatedAt = DateTime.UtcNow
        };

        requestType.ActiveVersionId = version.Id;

        db.RequestTypes.Add(requestType);
        db.RequestTypeVersions.Add(version);

        var response = new RequestResponse
        {
            Id = Guid.NewGuid(),
            RequestTypeVersionId = version.Id,
            UserId = _otherUserId,
            ResponseJson = "{}",
            IsComplete = false,
            StartedAt = DateTime.UtcNow
        };

        db.RequestResponses.Add(response);
        await db.SaveChangesAsync();

        return response.Id;
    }

    public async Task<Guid> CreateTestResponseWithFilesAsync()
    {
        var responseId = await CreateTestResponseForUserAsync();
        var fileId = await CreateTestFileAsync();

        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var file = await db.UploadedFiles.FindAsync(fileId);
        file!.RequestResponseId = responseId;
        await db.SaveChangesAsync();

        return responseId;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            // Clean up temp files
            if (Directory.Exists(_testFilesPath))
            {
                try
                {
                    Directory.Delete(_testFilesPath, true);
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }
        }
        base.Dispose(disposing);
    }
}
