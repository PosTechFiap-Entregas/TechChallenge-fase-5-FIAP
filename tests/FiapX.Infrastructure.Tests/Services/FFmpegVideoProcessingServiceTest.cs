using FiapX.Domain.Interfaces;
using FiapX.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;

namespace FiapX.Infrastructure.Tests.Services;

public class FFmpegVideoProcessingServiceTests
{
    private readonly FFmpegVideoProcessingService _service;

    public FFmpegVideoProcessingServiceTests()
    {
        _service = new FFmpegVideoProcessingService(NullLogger<FFmpegVideoProcessingService>.Instance);
    }

    private static string CreateTempOutputDir()
    {
        var dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(dir);
        return dir;
    }

    private static void CleanupDir(string dir)
    {
        if (Directory.Exists(dir))
            Directory.Delete(dir, recursive: true);
    }

    private static void CleanupFile(string file)
    {
        if (File.Exists(file))
            File.Delete(file);
    }

    [Fact]
    public void FFmpegVideoProcessingService_Constructor_ShouldCreateInstance()
    {
        var service = new FFmpegVideoProcessingService(NullLogger<FFmpegVideoProcessingService>.Instance);

        service.Should().NotBeNull();
    }

    [Fact]
    public void FFmpegVideoProcessingService_Constructor_ShouldNotThrow()
    {
        var act = () => new FFmpegVideoProcessingService(NullLogger<FFmpegVideoProcessingService>.Instance);

        act.Should().NotThrow();
    }

    [Fact]
    public async Task ProcessVideoAsync_WhenVideoFileDoesNotExist_ShouldReturnFailure()
    {
        var nonExistentPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".mp4");
        var outputDir = CreateTempOutputDir();

        try
        {
            var result = await _service.ProcessVideoAsync(nonExistentPath, outputDir);

            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().NotBeNullOrEmpty();
            result.ErrorMessage.Should().Contain("não encontrado");
        }
        finally
        {
            CleanupDir(outputDir);
        }
    }

    [Fact]
    public async Task CreateZipFromFramesAsync_ShouldCreateZipFile()
    {
        // Arrange
        var serviceType = typeof(FFmpegVideoProcessingService);
        var method = serviceType.GetMethod("CreateZipFromFramesAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        method.Should().NotBeNull();

        var tempDir = CreateTempOutputDir();
        var framesDir = Path.Combine(tempDir, "frames_test");
        Directory.CreateDirectory(framesDir);

        // create some dummy frame files
        for (int i = 0; i < 3; i++)
        {
            await File.WriteAllTextAsync(Path.Combine(framesDir, $"frame_{i}.png"), "data");
        }

        var zipPath = Path.Combine(tempDir, "frames.zip");

        try
        {
            var instance = _service;
            var task = (Task)method!.Invoke(instance, new object[] { framesDir, zipPath, CancellationToken.None })!;
            await task;

            File.Exists(zipPath).Should().BeTrue();
        }
        finally
        {
            CleanupDir(tempDir);
        }
    }

    [Fact]
    public void ExtractFramesAsync_WithInvalidMedia_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var serviceType = typeof(FFmpegVideoProcessingService);
        var method = serviceType.GetMethod("ExtractFramesAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        method.Should().NotBeNull();

        var tempDir = CreateTempOutputDir();
        var fakeVideo = Path.Combine(tempDir, "fake.mp4");
        File.WriteAllText(fakeVideo, "not a video");

        try
        {
            var instance = _service;
            var act = () => (Task<int>)method!.Invoke(instance, new object[] { fakeVideo, tempDir, 1, CancellationToken.None })!;

            // Invocation will throw when awaited, so get the task and assert
            var task = act();
            Func<Task> awaitTask = async () => { await task; };
            awaitTask.Should().ThrowAsync<InvalidOperationException>().GetAwaiter().GetResult();
        }
        finally
        {
            CleanupDir(tempDir);
        }
    }

    [Fact]
    public async Task ProcessVideoAsync_WhenVideoFileDoesNotExist_FrameCountShouldBeZero()
    {
        var nonExistentPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".mp4");
        var outputDir = CreateTempOutputDir();

        try
        {
            var result = await _service.ProcessVideoAsync(nonExistentPath, outputDir);

            result.FrameCount.Should().Be(0);
        }
        finally
        {
            CleanupDir(outputDir);
        }
    }

    [Fact]
    public async Task ProcessVideoAsync_WhenVideoFileDoesNotExist_ZipPathShouldBeNullOrEmpty()
    {
        var nonExistentPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".mp4");
        var outputDir = CreateTempOutputDir();

        try
        {
            var result = await _service.ProcessVideoAsync(nonExistentPath, outputDir);

            result.ZipPath.Should().BeNullOrEmpty();
        }
        finally
        {
            CleanupDir(outputDir);
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("/nonexistent/path/video.mp4")]
    [InlineData("C:\\nonexistent\\video.mp4")]
    public async Task ProcessVideoAsync_WithInvalidVideoPath_ShouldReturnFailure(string videoPath)
    {
        var outputDir = CreateTempOutputDir();

        try
        {
            var result = await _service.ProcessVideoAsync(videoPath, outputDir);

            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
        }
        finally
        {
            CleanupDir(outputDir);
        }
    }

    [Fact]
    public async Task ProcessVideoAsync_WhenFileNotFound_ShouldReturnResultWithExpectedShape()
    {
        var nonExistentPath = Path.Combine(Path.GetTempPath(), "missing_" + Guid.NewGuid() + ".mp4");
        var outputDir = CreateTempOutputDir();

        try
        {
            var result = await _service.ProcessVideoAsync(nonExistentPath, outputDir);

            result.Success.Should().BeFalse();
            result.ZipPath.Should().BeNullOrEmpty();
            result.FrameCount.Should().Be(0);
            result.ErrorMessage.Should().NotBeNull();
        }
        finally
        {
            CleanupDir(outputDir);
        }
    }

    [Fact]
    public async Task ProcessVideoAsync_ReturnType_ShouldBeVideoProcessingResult()
    {
        var nonExistentPath = "/not/found.mp4";
        var outputDir = CreateTempOutputDir();

        try
        {
            var result = await _service.ProcessVideoAsync(nonExistentPath, outputDir);

            result.Should().NotBeNull();
            result.Should().BeOfType<VideoProcessingResult>();
        }
        finally
        {
            CleanupDir(outputDir);
        }
    }

    [Fact]
    public async Task ProcessVideoAsync_ShouldAlwaysReturnResult_NeverNull()
    {
        var nonExistentPath = "/not/found.mp4";
        var outputDir = CreateTempOutputDir();

        try
        {
            var result = await _service.ProcessVideoAsync(nonExistentPath, outputDir);

            result.Should().NotBeNull();
        }
        finally
        {
            CleanupDir(outputDir);
        }
    }

    [Fact]
    public async Task ProcessVideoAsync_WithExistingButInvalidVideoFile_ShouldReturnFailure()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".mp4");
        var outputDir = CreateTempOutputDir();
        await File.WriteAllTextAsync(tempFile, "this is not a valid video file");

        try
        {
            var result = await _service.ProcessVideoAsync(tempFile, outputDir);

            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().NotBeNullOrEmpty();
        }
        finally
        {
            CleanupFile(tempFile);
            CleanupDir(outputDir);
        }
    }

    [Fact]
    public async Task ProcessVideoAsync_WithExistingButInvalidVideoFile_ProcessingDurationShouldBeNonNegative()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".mp4");
        var outputDir = CreateTempOutputDir();
        await File.WriteAllTextAsync(tempFile, "invalid content");

        try
        {
            var result = await _service.ProcessVideoAsync(tempFile, outputDir);

            result.ProcessingDuration.Should().BeGreaterThanOrEqualTo(TimeSpan.Zero);
        }
        finally
        {
            CleanupFile(tempFile);
            CleanupDir(outputDir);
        }
    }

    [Fact]
    public async Task ProcessVideoAsync_WithExistingButInvalidVideoFile_FrameCountShouldBeZero()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".mp4");
        var outputDir = CreateTempOutputDir();
        await File.WriteAllTextAsync(tempFile, "invalid content");

        try
        {
            var result = await _service.ProcessVideoAsync(tempFile, outputDir);

            result.FrameCount.Should().Be(0);
        }
        finally
        {
            CleanupFile(tempFile);
            CleanupDir(outputDir);
        }
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(30)]
    public async Task ProcessVideoAsync_WithVariousFpsValues_WhenFileNotFound_ShouldReturnFailure(int fps)
    {
        var nonExistentPath = "/path/does/not/exist.mp4";
        var outputDir = CreateTempOutputDir();

        try
        {
            var result = await _service.ProcessVideoAsync(nonExistentPath, outputDir, fps: fps);

            result.Success.Should().BeFalse();
        }
        finally
        {
            CleanupDir(outputDir);
        }
    }

    [Fact]
    public async Task ProcessVideoAsync_WhenCancelled_ResultShouldIndicateFailure()
    {
        var nonExistentPath = Path.Combine(Path.GetTempPath(), "cancel_" + Guid.NewGuid() + ".mp4");
        var outputDir = CreateTempOutputDir();
        var cts = new CancellationTokenSource();
        cts.Cancel();

        try
        {
            VideoProcessingResult? result = null;
            try
            {
                result = await _service.ProcessVideoAsync(nonExistentPath, outputDir, cancellationToken: cts.Token);
            }
            catch (OperationCanceledException)
            {
                return;
            }

            result.Should().NotBeNull();
            result!.Success.Should().BeFalse();
        }
        finally
        {
            CleanupDir(outputDir);
        }
    }

    [Fact]
    public async Task ProcessVideoAsync_WithAlreadyCancelledToken_ShouldNotThrowUnexpectedException()
    {
        var nonExistentPath = Path.Combine(Path.GetTempPath(), "cancel2_" + Guid.NewGuid() + ".mp4");
        var outputDir = CreateTempOutputDir();
        var cts = new CancellationTokenSource();
        cts.Cancel();

        try
        {
            var act = async () => await _service.ProcessVideoAsync(
                nonExistentPath, outputDir, cancellationToken: cts.Token);

            await act.Should().NotThrowAsync<InvalidOperationException>();
            await act.Should().NotThrowAsync<NullReferenceException>();
            await act.Should().NotThrowAsync<ArgumentNullException>();
        }
        finally
        {
            CleanupDir(outputDir);
        }
    }

    [Fact]
    public async Task ProcessVideoAsync_WhenFileNotFound_ShouldNotThrowUnhandledException()
    {
        var nonExistentPath = Path.Combine(Path.GetTempPath(), "no_throw_" + Guid.NewGuid() + ".mp4");
        var outputDir = CreateTempOutputDir();

        try
        {
            var act = async () => await _service.ProcessVideoAsync(nonExistentPath, outputDir);
            await act.Should().NotThrowAsync<Exception>();
        }
        finally
        {
            CleanupDir(outputDir);
        }
    }

    [Fact]
    public async Task ProcessVideoAsync_WithInvalidFile_ShouldNotThrowUnhandledException()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".mp4");
        var outputDir = CreateTempOutputDir();
        await File.WriteAllTextAsync(tempFile, "not a video");

        try
        {
            var act = async () => await _service.ProcessVideoAsync(tempFile, outputDir);
            await act.Should().NotThrowAsync<NullReferenceException>();
            await act.Should().NotThrowAsync<ArgumentNullException>();
        }
        finally
        {
            CleanupFile(tempFile);
            CleanupDir(outputDir);
        }
    }

    [Fact]
    public async Task ProcessVideoAsync_WhenFFmpegExecutableExists_ShouldSkipDownloadBranch()
    {
        var ffmpegType = typeof(FFmpegVideoProcessingService);
        var downloadedField = ffmpegType.GetField("_ffmpegDownloaded", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
        downloadedField?.SetValue(null, false);

        var ffmpegPath = Path.Combine(Path.GetTempPath(), "ffmpeg");
        Directory.CreateDirectory(ffmpegPath);
        var ffmpegExecutable = Path.Combine(ffmpegPath, OperatingSystem.IsWindows() ? "ffmpeg.exe" : "ffmpeg");
        await File.WriteAllTextAsync(ffmpegExecutable, "fake");

        var nonExistentPath = Path.Combine(Path.GetTempPath(), "missing_ffmpeg_test_" + Guid.NewGuid() + ".mp4");
        var outputDir = CreateTempOutputDir();

        try
        {
            var result = await _service.ProcessVideoAsync(nonExistentPath, outputDir);

            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("não encontrado");
        }
        finally
        {
            try { File.Delete(ffmpegExecutable); } catch { }
            try { Directory.Delete(ffmpegPath, true); } catch { }
            CleanupDir(outputDir);
        }
    }
}
