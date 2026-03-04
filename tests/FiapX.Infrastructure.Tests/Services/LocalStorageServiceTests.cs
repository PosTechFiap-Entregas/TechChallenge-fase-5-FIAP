using FiapX.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;

namespace FiapX.Infrastructure.Tests.Services;

public class LocalStorageServiceTests : IDisposable
{
    private readonly LocalStorageService _sut;
    private readonly string _testBasePath;

    public LocalStorageServiceTests()
    {
        _testBasePath = Path.Combine(Path.GetTempPath(), "fiapx-test-" + Guid.NewGuid());

        var configData = new Dictionary<string, string>
        {
            { "Storage:BasePath", _testBasePath }
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData!)
            .Build();

        _sut = new LocalStorageService(configuration);
    }

    [Fact]
    public async Task SaveVideoAsync_WithValidStream_ShouldSaveFile()
    {
        // Arrange
        var content = "test video content"u8.ToArray();
        var stream = new MemoryStream(content);
        var fileName = "test-video.mp4";

        // Act
        var filePath = await _sut.SaveVideoAsync(stream, fileName);

        // Assert
        filePath.Should().NotBeNullOrEmpty();
        File.Exists(filePath).Should().BeTrue();

        var savedContent = await File.ReadAllBytesAsync(filePath);
        savedContent.Should().Equal(content);
    }

    [Fact]
    public async Task SaveZipAsync_WithValidStream_ShouldSaveFile()
    {
        // Arrange
        var content = "test zip content"u8.ToArray();
        var stream = new MemoryStream(content);
        var fileName = "frames.zip";

        // Act
        var filePath = await _sut.SaveZipAsync(stream, fileName);

        // Assert
        filePath.Should().NotBeNullOrEmpty();
        File.Exists(filePath).Should().BeTrue();
    }

    [Fact]
    public async Task GetFileAsync_WithExistingFile_ShouldReturnStream()
    {
        // Arrange
        var content = "test content"u8.ToArray();
        var stream = new MemoryStream(content);
        var filePath = await _sut.SaveVideoAsync(stream, "test.mp4");

        // Act
        var resultStream = await _sut.GetFileAsync(filePath);

        // Assert
        resultStream.Should().NotBeNull();

        using var ms = new MemoryStream();
        await resultStream.CopyToAsync(ms);
        ms.ToArray().Should().Equal(content);
    }

    [Fact]
    public async Task GetFileAsync_WithNonExistentFile_ShouldThrowException()
    {
        // Arrange
        var nonExistentPath = Path.Combine(_testBasePath, "non-existent.mp4");

        // Act
        var act = () => _sut.GetFileAsync(nonExistentPath);

        // Assert
        await act.Should().ThrowAsync<FileNotFoundException>();
    }

    [Fact]
    public async Task DeleteFileAsync_WithExistingFile_ShouldDeleteFile()
    {
        // Arrange
        var stream = new MemoryStream("content"u8.ToArray());
        var filePath = await _sut.SaveVideoAsync(stream, "to-delete.mp4");
        File.Exists(filePath).Should().BeTrue();

        // Act
        await _sut.DeleteFileAsync(filePath);

        // Assert
        File.Exists(filePath).Should().BeFalse();
    }

    [Fact]
    public async Task FileExistsAsync_WithExistingFile_ShouldReturnTrue()
    {
        // Arrange
        var stream = new MemoryStream("content"u8.ToArray());
        var filePath = await _sut.SaveVideoAsync(stream, "exists.mp4");

        // Act
        var exists = await _sut.FileExistsAsync(filePath);

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task GetFileSizeAsync_ShouldReturnCorrectSize()
    {
        // Arrange
        var content = "test content with specific size"u8.ToArray();
        var stream = new MemoryStream(content);
        var filePath = await _sut.SaveVideoAsync(stream, "sized.mp4");

        // Act
        var size = await _sut.GetFileSizeAsync(filePath);

        // Assert
        size.Should().Be(content.Length);
    }

    [Fact]
    public void GetTempDirectory_ShouldCreateUniqueDirectory()
    {
        // Act
        var tempDir1 = _sut.GetTempDirectory();
        var tempDir2 = _sut.GetTempDirectory();

        // Assert
        tempDir1.Should().NotBe(tempDir2);
        Directory.Exists(tempDir1).Should().BeTrue();
        Directory.Exists(tempDir2).Should().BeTrue();
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
}