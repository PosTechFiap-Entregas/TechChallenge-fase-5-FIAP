using FiapX.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using System.Linq;

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
        var content = "test video content"u8.ToArray();
        var stream = new MemoryStream(content);
        var fileName = "test-video.mp4";

        var filePath = await _sut.SaveVideoAsync(stream, fileName);

        filePath.Should().NotBeNullOrEmpty();
        File.Exists(filePath).Should().BeTrue();

        var savedContent = await File.ReadAllBytesAsync(filePath);
        savedContent.Should().Equal(content);
    }

    [Fact]
    public async Task SaveZipAsync_WithValidStream_ShouldSaveFile()
    {
        var content = "test zip content"u8.ToArray();
        var stream = new MemoryStream(content);
        var fileName = "frames.zip";

        var filePath = await _sut.SaveZipAsync(stream, fileName);

        filePath.Should().NotBeNullOrEmpty();
        File.Exists(filePath).Should().BeTrue();
    }

    [Fact]
    public async Task GetFileAsync_WithExistingFile_ShouldReturnStream()
    {
        var content = "test content"u8.ToArray();
        var stream = new MemoryStream(content);
        var filePath = await _sut.SaveVideoAsync(stream, "test.mp4");

        var resultStream = await _sut.GetFileAsync(filePath);

        resultStream.Should().NotBeNull();

        using var ms = new MemoryStream();
        await resultStream.CopyToAsync(ms);
        ms.ToArray().Should().Equal(content);
    }

    [Fact]
    public async Task GetFileAsync_WithNonExistentFile_ShouldThrowException()
    {
        var nonExistentPath = Path.Combine(_testBasePath, "non-existent.mp4");

        var act = () => _sut.GetFileAsync(nonExistentPath);

        await act.Should().ThrowAsync<FileNotFoundException>();
    }

    [Fact]
    public async Task DeleteFileAsync_WithExistingFile_ShouldDeleteFile()
    {
        var stream = new MemoryStream("content"u8.ToArray());
        var filePath = await _sut.SaveVideoAsync(stream, "to-delete.mp4");
        File.Exists(filePath).Should().BeTrue();

        await _sut.DeleteFileAsync(filePath);

        File.Exists(filePath).Should().BeFalse();
    }

    [Fact]
    public async Task FileExistsAsync_WithExistingFile_ShouldReturnTrue()
    {
        var stream = new MemoryStream("content"u8.ToArray());
        var filePath = await _sut.SaveVideoAsync(stream, "exists.mp4");

        var exists = await _sut.FileExistsAsync(filePath);

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task GetFileSizeAsync_ShouldReturnCorrectSize()
    {
        var content = "test content with specific size"u8.ToArray();
        var stream = new MemoryStream(content);
        var filePath = await _sut.SaveVideoAsync(stream, "sized.mp4");

        var size = await _sut.GetFileSizeAsync(filePath);

        size.Should().Be(content.Length);
    }

    [Fact]
    public void GetTempDirectory_ShouldCreateUniqueDirectory()
    {
        var tempDir1 = _sut.GetTempDirectory();
        var tempDir2 = _sut.GetTempDirectory();

        tempDir1.Should().NotBe(tempDir2);
        Directory.Exists(tempDir1).Should().BeTrue();
        Directory.Exists(tempDir2).Should().BeTrue();
    }

    [Fact]
    public async Task SaveVideoAsync_WithInvalidFileName_ShouldSanitizeFileName()
    {
        var content = "video"u8.ToArray();
        var stream = new MemoryStream(content);
        var invalidName = "inva|id:na*me?.mp4";

        var path = await _sut.SaveVideoAsync(stream, invalidName);

        path.Should().NotBeNullOrEmpty();
        var fileName = Path.GetFileName(path);
        fileName.Should().NotContainAny(Path.GetInvalidFileNameChars().Select(c => c.ToString()).ToArray());
        fileName.Should().Contain("_");
        File.Exists(path).Should().BeTrue();
    }

    [Fact]
    public async Task CleanupTempFilesAsync_ShouldRemoveOldDirectories()
    {
        var tempRoot = Path.Combine(_testBasePath, "temp");
        Directory.CreateDirectory(tempRoot);

        var oldDir = Path.Combine(tempRoot, Guid.NewGuid().ToString());
        Directory.CreateDirectory(oldDir);

        Directory.SetCreationTimeUtc(oldDir, DateTime.UtcNow.AddHours(-3));

        var recentDir = Path.Combine(tempRoot, Guid.NewGuid().ToString());
        Directory.CreateDirectory(recentDir);
        Directory.SetCreationTimeUtc(recentDir, DateTime.UtcNow);

        await _sut.CleanupTempFilesAsync();

        Directory.Exists(oldDir).Should().BeFalse();
        Directory.Exists(recentDir).Should().BeTrue();
    }

    [Fact]
    public async Task GetFileSizeAsync_NonExistentFile_ShouldThrow()
    {
        var nonExistent = Path.Combine(_testBasePath, "does-not-exist.mp4");

        var act = () => _sut.GetFileSizeAsync(nonExistent);

        await act.Should().ThrowAsync<FileNotFoundException>();
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
               
            }
        }
    }
}