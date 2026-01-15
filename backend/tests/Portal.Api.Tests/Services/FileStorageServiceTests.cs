using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Portal.Api.Tests.Services;

public class FileStorageServiceTests : IDisposable
{
    private readonly string _testBasePath;
    private readonly LocalFileStorageService _service;
    private readonly Mock<ILogger<LocalFileStorageService>> _loggerMock;

    public FileStorageServiceTests()
    {
        _testBasePath = Path.Combine(Path.GetTempPath(), "file-storage-tests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testBasePath);

        var options = Options.Create(new FileStorageOptions
        {
            BasePath = _testBasePath,
            MaxFileSizeBytes = 25 * 1024 * 1024 // 25 MB
        });

        _loggerMock = new Mock<ILogger<LocalFileStorageService>>();
        _service = new LocalFileStorageService(options, _loggerMock.Object);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testBasePath))
        {
            try
            {
                Directory.Delete(_testBasePath, true);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }

    #region ValidateFile Tests

    [Fact]
    public void ValidateFile_WithValidTextFile_ReturnsValid()
    {
        // Arrange
        var file = CreateMockFile("test.txt", "text/plain", 100);

        // Act
        var (isValid, error) = _service.ValidateFile(file);

        // Assert
        isValid.Should().BeTrue();
        error.Should().BeNull();
    }

    [Fact]
    public void ValidateFile_WithValidPdfFile_ReturnsValid()
    {
        // Arrange
        var file = CreateMockFile("document.pdf", "application/pdf", 1024);

        // Act
        var (isValid, error) = _service.ValidateFile(file);

        // Assert
        isValid.Should().BeTrue();
        error.Should().BeNull();
    }

    [Fact]
    public void ValidateFile_WithValidImageFile_ReturnsValid()
    {
        // Arrange
        var file = CreateMockFile("photo.jpg", "image/jpeg", 2048);

        // Act
        var (isValid, error) = _service.ValidateFile(file);

        // Assert
        isValid.Should().BeTrue();
        error.Should().BeNull();
    }

    [Fact]
    public void ValidateFile_WithFileTooLarge_ReturnsInvalid()
    {
        // Arrange
        var file = CreateMockFile("large.txt", "text/plain", 30 * 1024 * 1024); // 30 MB

        // Act
        var (isValid, error) = _service.ValidateFile(file);

        // Assert
        isValid.Should().BeFalse();
        error.Should().Contain("exceeds maximum allowed size");
    }

    [Fact]
    public void ValidateFile_WithEmptyFile_ReturnsInvalid()
    {
        // Arrange
        var file = CreateMockFile("empty.txt", "text/plain", 0);

        // Act
        var (isValid, error) = _service.ValidateFile(file);

        // Assert
        isValid.Should().BeFalse();
        error.Should().Be("File is empty");
    }

    [Fact]
    public void ValidateFile_WithDisallowedExtension_ReturnsInvalid()
    {
        // Arrange
        var file = CreateMockFile("script.exe", "application/x-executable", 100);

        // Act
        var (isValid, error) = _service.ValidateFile(file);

        // Assert
        isValid.Should().BeFalse();
        error.Should().Contain("is not allowed");
    }

    [Fact]
    public void ValidateFile_WithNoExtension_ReturnsInvalid()
    {
        // Arrange
        var file = CreateMockFile("noextension", "text/plain", 100);

        // Act
        var (isValid, error) = _service.ValidateFile(file);

        // Assert
        isValid.Should().BeFalse();
        error.Should().Contain("is not allowed");
    }

    [Fact]
    public void ValidateFile_WithDisallowedMimeType_ReturnsInvalid()
    {
        // Arrange
        var file = CreateMockFile("file.txt", "application/x-executable", 100);

        // Act
        var (isValid, error) = _service.ValidateFile(file);

        // Assert
        isValid.Should().BeFalse();
        error.Should().Contain("Content type");
    }

    [Theory]
    [InlineData(".pdf", "application/pdf")]
    [InlineData(".doc", "application/msword")]
    [InlineData(".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document")]
    [InlineData(".xls", "application/vnd.ms-excel")]
    [InlineData(".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")]
    [InlineData(".txt", "text/plain")]
    [InlineData(".jpg", "image/jpeg")]
    [InlineData(".jpeg", "image/jpeg")]
    [InlineData(".png", "image/png")]
    [InlineData(".gif", "image/gif")]
    public void ValidateFile_WithAllowedTypes_ReturnsValid(string extension, string mimeType)
    {
        // Arrange
        var file = CreateMockFile($"file{extension}", mimeType, 100);

        // Act
        var (isValid, error) = _service.ValidateFile(file);

        // Assert
        isValid.Should().BeTrue();
        error.Should().BeNull();
    }

    #endregion

    #region SaveFileAsync Tests

    [Fact]
    public async Task SaveFileAsync_WithValidTextFile_SavesSuccessfully()
    {
        // Arrange
        var content = "This is test content";
        var file = CreateMockFileWithContent("test.txt", "text/plain", content);
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        // Act
        var result = await _service.SaveFileAsync(tenantId, userId, "question1", file);

        // Assert
        result.Should().NotBeNull();
        result.TenantId.Should().Be(tenantId);
        result.UserId.Should().Be(userId);
        result.QuestionName.Should().Be("question1");
        result.OriginalFileName.Should().Be("test.txt");
        result.ContentType.Should().Be("text/plain");
        result.FileSizeBytes.Should().Be(content.Length);

        // Verify file was saved
        var fullPath = Path.Combine(_testBasePath, result.StoragePath);
        File.Exists(fullPath).Should().BeTrue();
        (await File.ReadAllTextAsync(fullPath)).Should().Be(content);
    }

    [Fact]
    public async Task SaveFileAsync_WithPdfFile_ValidatesMagicBytes()
    {
        // Arrange
        var pdfContent = new byte[] { 0x25, 0x50, 0x44, 0x46, 0x2D, 0x31, 0x2E, 0x34 }; // %PDF-1.4
        var file = CreateMockFileWithBytes("document.pdf", "application/pdf", pdfContent);
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        // Act
        var result = await _service.SaveFileAsync(tenantId, userId, "document", file);

        // Assert
        result.Should().NotBeNull();
        result.OriginalFileName.Should().Be("document.pdf");
    }

    [Fact]
    public async Task SaveFileAsync_WithPngFile_ValidatesMagicBytes()
    {
        // Arrange
        var pngContent = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }; // PNG signature
        var file = CreateMockFileWithBytes("image.png", "image/png", pngContent);
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        // Act
        var result = await _service.SaveFileAsync(tenantId, userId, "image", file);

        // Assert
        result.Should().NotBeNull();
        result.OriginalFileName.Should().Be("image.png");
    }

    [Fact]
    public async Task SaveFileAsync_WithInvalidMagicBytes_ThrowsArgumentException()
    {
        // Arrange
        var invalidContent = new byte[] { 0x00, 0x00, 0x00, 0x00 }; // Not a valid PDF
        var file = CreateMockFileWithBytes("fake.pdf", "application/pdf", invalidContent);
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        // Act
        var act = async () => await _service.SaveFileAsync(tenantId, userId, "document", file);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("File content does not match its extension");
    }

    [Fact]
    public async Task SaveFileAsync_WithInvalidFile_ThrowsArgumentException()
    {
        // Arrange
        var file = CreateMockFile("script.exe", "application/x-executable", 100);
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        // Act
        var act = async () => await _service.SaveFileAsync(tenantId, userId, "question1", file);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task SaveFileAsync_SanitizesFileName()
    {
        // Arrange
        var content = "test content";
        var file = CreateMockFileWithContent("test<file>.txt", "text/plain", content);
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        // Act
        var result = await _service.SaveFileAsync(tenantId, userId, "question1", file);

        // Assert
        result.OriginalFileName.Should().NotContain("<");
        result.OriginalFileName.Should().NotContain(">");
    }

    [Fact]
    public async Task SaveFileAsync_CreatesCorrectDirectoryStructure()
    {
        // Arrange
        var content = "test content";
        var file = CreateMockFileWithContent("test.txt", "text/plain", content);
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        // Act
        var result = await _service.SaveFileAsync(tenantId, userId, "question1", file);

        // Assert
        result.StoragePath.Should().Contain(tenantId.ToString());
        result.StoragePath.Should().Contain(DateTime.UtcNow.Year.ToString());
        result.StoragePath.Should().Contain(DateTime.UtcNow.Month.ToString("D2"));
    }

    #endregion

    #region GetFileStreamAsync Tests

    [Fact]
    public async Task GetFileStreamAsync_WithExistingFile_ReturnsStream()
    {
        // Arrange
        var content = "test content for stream";
        var file = CreateMockFileWithContent("test.txt", "text/plain", content);
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var savedFile = await _service.SaveFileAsync(tenantId, userId, "question1", file);

        // Act
        var stream = await _service.GetFileStreamAsync(savedFile);

        // Assert
        stream.Should().NotBeNull();
        using var reader = new StreamReader(stream);
        var readContent = await reader.ReadToEndAsync();
        readContent.Should().Be(content);
    }

    [Fact]
    public async Task GetFileStreamAsync_WithNonExistentFile_ThrowsFileNotFoundException()
    {
        // Arrange
        var uploadedFile = new UploadedFile
        {
            Id = Guid.NewGuid(),
            StoragePath = "nonexistent/path/file.txt"
        };

        // Act
        var act = async () => await _service.GetFileStreamAsync(uploadedFile);

        // Assert
        await act.Should().ThrowAsync<FileNotFoundException>();
    }

    #endregion

    #region DeleteFileAsync Tests

    [Fact]
    public async Task DeleteFileAsync_WithExistingFile_DeletesFile()
    {
        // Arrange
        var content = "file to delete";
        var file = CreateMockFileWithContent("delete-me.txt", "text/plain", content);
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var savedFile = await _service.SaveFileAsync(tenantId, userId, "question1", file);
        var fullPath = Path.Combine(_testBasePath, savedFile.StoragePath);
        File.Exists(fullPath).Should().BeTrue();

        // Act
        await _service.DeleteFileAsync(savedFile);

        // Assert
        File.Exists(fullPath).Should().BeFalse();
    }

    [Fact]
    public async Task DeleteFileAsync_WithNonExistentFile_DoesNotThrow()
    {
        // Arrange
        var uploadedFile = new UploadedFile
        {
            Id = Guid.NewGuid(),
            StoragePath = "nonexistent/path/file.txt"
        };

        // Act
        var act = async () => await _service.DeleteFileAsync(uploadedFile);

        // Assert
        await act.Should().NotThrowAsync();
    }

    #endregion

    #region Helper Methods

    private static IFormFile CreateMockFile(string fileName, string contentType, long length)
    {
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.FileName).Returns(fileName);
        mockFile.Setup(f => f.ContentType).Returns(contentType);
        mockFile.Setup(f => f.Length).Returns(length);
        return mockFile.Object;
    }

    private static IFormFile CreateMockFileWithContent(string fileName, string contentType, string content)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(content);
        return CreateMockFileWithBytes(fileName, contentType, bytes);
    }

    private static IFormFile CreateMockFileWithBytes(string fileName, string contentType, byte[] bytes)
    {
        var stream = new MemoryStream(bytes);
        var mockFile = new Mock<IFormFile>();

        mockFile.Setup(f => f.FileName).Returns(fileName);
        mockFile.Setup(f => f.ContentType).Returns(contentType);
        mockFile.Setup(f => f.Length).Returns(bytes.Length);
        mockFile.Setup(f => f.OpenReadStream()).Returns(() => new MemoryStream(bytes));
        mockFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .Returns<Stream, CancellationToken>(async (target, ct) =>
            {
                await new MemoryStream(bytes).CopyToAsync(target, ct);
            });

        return mockFile.Object;
    }

    #endregion
}
