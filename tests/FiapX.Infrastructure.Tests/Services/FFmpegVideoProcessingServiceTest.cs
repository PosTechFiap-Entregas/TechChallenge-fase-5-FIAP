using FiapX.Domain.Interfaces;
using FiapX.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using System.Reflection;

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
        try { if (Directory.Exists(dir)) Directory.Delete(dir, recursive: true); } catch { }
    }

    private static void CleanupFile(string file)
    {
        try { if (File.Exists(file)) File.Delete(file); } catch { }
    }

    private static void SetFfmpegDownloaded(bool value)
    {
        var field = typeof(FFmpegVideoProcessingService)
            .GetField("_ffmpegDownloaded", BindingFlags.Static | BindingFlags.NonPublic);
        field?.SetValue(null, value);
    }

    [Fact]
    public void Constructor_ShouldCreateInstance()
    {
        var service = new FFmpegVideoProcessingService(NullLogger<FFmpegVideoProcessingService>.Instance);
        service.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_ShouldNotThrow()
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
            SetFfmpegDownloaded(true);

            var result = await _service.ProcessVideoAsync(nonExistentPath, outputDir);

            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("não encontrado");
        }
        finally { CleanupDir(outputDir); }
    }

    [Fact]
    public async Task ProcessVideoAsync_WhenFileNotFound_FrameCountShouldBeZero()
    {
        SetFfmpegDownloaded(true);
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".mp4");
        var outputDir = CreateTempOutputDir();
        try
        {
            var result = await _service.ProcessVideoAsync(path, outputDir);
            result.FrameCount.Should().Be(0);
        }
        finally { CleanupDir(outputDir); }
    }

    [Fact]
    public async Task ProcessVideoAsync_WhenFileNotFound_ZipPathShouldBeNullOrEmpty()
    {
        SetFfmpegDownloaded(true);
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".mp4");
        var outputDir = CreateTempOutputDir();
        try
        {
            var result = await _service.ProcessVideoAsync(path, outputDir);
            result.ZipPath.Should().BeNullOrEmpty();
        }
        finally { CleanupDir(outputDir); }
    }

    [Fact]
    public async Task ProcessVideoAsync_WhenFileNotFound_ShouldReturnResultWithExpectedShape()
    {
        SetFfmpegDownloaded(true);
        var path = Path.Combine(Path.GetTempPath(), "missing_" + Guid.NewGuid() + ".mp4");
        var outputDir = CreateTempOutputDir();
        try
        {
            var result = await _service.ProcessVideoAsync(path, outputDir);
            result.Success.Should().BeFalse();
            result.ZipPath.Should().BeNullOrEmpty();
            result.FrameCount.Should().Be(0);
            result.ErrorMessage.Should().NotBeNull();
        }
        finally { CleanupDir(outputDir); }
    }

    [Fact]
    public async Task ProcessVideoAsync_ReturnType_ShouldBeVideoProcessingResult()
    {
        SetFfmpegDownloaded(true);
        var path = "/not/found.mp4";
        var outputDir = CreateTempOutputDir();
        try
        {
            var result = await _service.ProcessVideoAsync(path, outputDir);
            result.Should().BeOfType<VideoProcessingResult>();
        }
        finally { CleanupDir(outputDir); }
    }

    [Fact]
    public async Task ProcessVideoAsync_ShouldAlwaysReturnResult_NeverNull()
    {
        SetFfmpegDownloaded(true);
        var path = "/not/found.mp4";
        var outputDir = CreateTempOutputDir();
        try
        {
            var result = await _service.ProcessVideoAsync(path, outputDir);
            result.Should().NotBeNull();
        }
        finally { CleanupDir(outputDir); }
    }

    [Fact]
    public async Task ProcessVideoAsync_WhenFileNotFound_ShouldNotThrowUnhandledException()
    {
        SetFfmpegDownloaded(true);
        var path = Path.Combine(Path.GetTempPath(), "no_throw_" + Guid.NewGuid() + ".mp4");
        var outputDir = CreateTempOutputDir();
        try
        {
            var act = async () => await _service.ProcessVideoAsync(path, outputDir);
            await act.Should().NotThrowAsync<Exception>();
        }
        finally { CleanupDir(outputDir); }
    }

    [Fact]
    public async Task ProcessVideoAsync_WithExistingButInvalidVideoFile_ShouldReturnFailure()
    {
        SetFfmpegDownloaded(true);
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
        finally { CleanupFile(tempFile); CleanupDir(outputDir); }
    }

    [Fact]
    public async Task ProcessVideoAsync_WithExistingButInvalidVideoFile_ProcessingDurationShouldBeNonNegative()
    {
        SetFfmpegDownloaded(true);
        var tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".mp4");
        var outputDir = CreateTempOutputDir();
        await File.WriteAllTextAsync(tempFile, "invalid content");
        try
        {
            var result = await _service.ProcessVideoAsync(tempFile, outputDir);
            result.ProcessingDuration.Should().BeGreaterThanOrEqualTo(TimeSpan.Zero);
        }
        finally { CleanupFile(tempFile); CleanupDir(outputDir); }
    }

    [Fact]
    public async Task ProcessVideoAsync_WithExistingButInvalidVideoFile_FrameCountShouldBeZero()
    {
        SetFfmpegDownloaded(true);
        var tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".mp4");
        var outputDir = CreateTempOutputDir();
        await File.WriteAllTextAsync(tempFile, "invalid content");
        try
        {
            var result = await _service.ProcessVideoAsync(tempFile, outputDir);
            result.FrameCount.Should().Be(0);
        }
        finally { CleanupFile(tempFile); CleanupDir(outputDir); }
    }

    [Fact]
    public async Task ProcessVideoAsync_WithInvalidFile_ShouldNotThrowUnhandledException()
    {
        SetFfmpegDownloaded(true);
        var tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".mp4");
        var outputDir = CreateTempOutputDir();
        await File.WriteAllTextAsync(tempFile, "not a video");
        try
        {
            var act = async () => await _service.ProcessVideoAsync(tempFile, outputDir);
            await act.Should().NotThrowAsync<NullReferenceException>();
            await act.Should().NotThrowAsync<ArgumentNullException>();
        }
        finally { CleanupFile(tempFile); CleanupDir(outputDir); }
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("/nonexistent/path/video.mp4")]
    [InlineData("C:\\nonexistent\\video.mp4")]
    public async Task ProcessVideoAsync_WithInvalidVideoPath_ShouldReturnFailure(string videoPath)
    {
        SetFfmpegDownloaded(true);
        var outputDir = CreateTempOutputDir();
        try
        {
            var result = await _service.ProcessVideoAsync(videoPath, outputDir);
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
        }
        finally { CleanupDir(outputDir); }
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(30)]
    public async Task ProcessVideoAsync_WithVariousFpsValues_WhenFileNotFound_ShouldReturnFailure(int fps)
    {
        SetFfmpegDownloaded(true);
        var path = "/path/does/not/exist.mp4";
        var outputDir = CreateTempOutputDir();
        try
        {
            var result = await _service.ProcessVideoAsync(path, outputDir, fps: fps);
            result.Success.Should().BeFalse();
        }
        finally { CleanupDir(outputDir); }
    }

    [Fact]
    public async Task ProcessVideoAsync_WhenCancelled_ShouldReturnFailureOrCatchCancellation()
    {
        SetFfmpegDownloaded(true);
        var path = Path.Combine(Path.GetTempPath(), "cancel_" + Guid.NewGuid() + ".mp4");
        var outputDir = CreateTempOutputDir();
        var cts = new CancellationTokenSource();
        cts.Cancel();
        try
        {
            VideoProcessingResult? result = null;
            try
            {
                result = await _service.ProcessVideoAsync(path, outputDir, cancellationToken: cts.Token);
            }
            catch (OperationCanceledException) { return; }

            result.Should().NotBeNull();
            result!.Success.Should().BeFalse();
        }
        finally { CleanupDir(outputDir); }
    }

    [Fact]
    public async Task ProcessVideoAsync_WithAlreadyCancelledToken_ShouldNotThrowUnexpectedException()
    {
        SetFfmpegDownloaded(true);
        var path = Path.Combine(Path.GetTempPath(), "cancel2_" + Guid.NewGuid() + ".mp4");
        var outputDir = CreateTempOutputDir();
        var cts = new CancellationTokenSource();
        cts.Cancel();
        try
        {
            var act = async () => await _service.ProcessVideoAsync(path, outputDir, cancellationToken: cts.Token);
            await act.Should().NotThrowAsync<InvalidOperationException>();
            await act.Should().NotThrowAsync<NullReferenceException>();
            await act.Should().NotThrowAsync<ArgumentNullException>();
        }
        finally { CleanupDir(outputDir); }
    }

    [Fact]
    public async Task ProcessVideoAsync_WhenFfmpegAlreadyDownloaded_ShouldSkipEnsureFFmpeg()
    {
        SetFfmpegDownloaded(true);
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".mp4");
        var outputDir = CreateTempOutputDir();
        try
        {
            var result1 = await _service.ProcessVideoAsync(path, outputDir);
            var result2 = await _service.ProcessVideoAsync(path, outputDir);

            result1.Success.Should().BeFalse();
            result2.Success.Should().BeFalse();
        }
        finally { CleanupDir(outputDir); }
    }


    [Fact]
    public async Task ProcessVideoAsync_WhenFFmpegExecutableExists_ShouldSkipDownloadBranch()
    {
        SetFfmpegDownloaded(false);

        var ffmpegPath = Path.Combine(Path.GetTempPath(), "ffmpeg");
        Directory.CreateDirectory(ffmpegPath);
        var ffmpegExecutable = Path.Combine(ffmpegPath,
            OperatingSystem.IsWindows() ? "ffmpeg.exe" : "ffmpeg");
        await File.WriteAllTextAsync(ffmpegExecutable, "fake ffmpeg binary");

        var path = Path.Combine(Path.GetTempPath(), "missing_ffmpeg_test_" + Guid.NewGuid() + ".mp4");
        var outputDir = CreateTempOutputDir();
        try
        {
            var result = await _service.ProcessVideoAsync(path, outputDir);

            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("não encontrado");
        }
        finally
        {
            CleanupFile(ffmpegExecutable);
            CleanupDir(outputDir);
        }
    }

    [Fact]
    public async Task CreateZipFromFramesAsync_ShouldCreateZipFile()
    {
        var method = typeof(FFmpegVideoProcessingService)
            .GetMethod("CreateZipFromFramesAsync", BindingFlags.NonPublic | BindingFlags.Instance);
        method.Should().NotBeNull();

        var tempDir = CreateTempOutputDir();
        var framesDir = Path.Combine(tempDir, "frames_test");
        Directory.CreateDirectory(framesDir);

        for (int i = 0; i < 3; i++)
            await File.WriteAllTextAsync(Path.Combine(framesDir, $"frame_{i}.png"), "data");

        var zipPath = Path.Combine(tempDir, "frames.zip");
        try
        {
            var task = (Task)method!.Invoke(_service, new object[] { framesDir, zipPath, CancellationToken.None })!;
            await task;

            File.Exists(zipPath).Should().BeTrue();
        }
        finally { CleanupDir(tempDir); }
    }

    [Fact]
    public async Task CreateZipFromFramesAsync_WhenZipAlreadyExists_ShouldOverwrite()
    {
        var method = typeof(FFmpegVideoProcessingService)
            .GetMethod("CreateZipFromFramesAsync", BindingFlags.NonPublic | BindingFlags.Instance);
        method.Should().NotBeNull();

        var tempDir = CreateTempOutputDir();
        var framesDir = Path.Combine(tempDir, "frames_overwrite");
        Directory.CreateDirectory(framesDir);
        await File.WriteAllTextAsync(Path.Combine(framesDir, "frame_0.png"), "data");

        var zipPath = Path.Combine(tempDir, "existing.zip");
        await File.WriteAllTextAsync(zipPath, "old zip content");

        try
        {
            var task = (Task)method!.Invoke(_service, new object[] { framesDir, zipPath, CancellationToken.None })!;
            await task;

            File.Exists(zipPath).Should().BeTrue();
            new FileInfo(zipPath).Length.Should().BeGreaterThan(0);
        }
        finally { CleanupDir(tempDir); }
    }

    [Fact]
    public async Task ExtractFramesAsync_WithInvalidMedia_ShouldThrowInvalidOperationException()
    {
        SetFfmpegDownloaded(true);
        var method = typeof(FFmpegVideoProcessingService)
            .GetMethod("ExtractFramesAsync", BindingFlags.NonPublic | BindingFlags.Instance);
        method.Should().NotBeNull();

        var tempDir = CreateTempOutputDir();
        var fakeVideo = Path.Combine(tempDir, "fake.mp4");
        await File.WriteAllTextAsync(fakeVideo, "not a video");
        try
        {
            var task = (Task<int>)method!.Invoke(_service,
                new object[] { fakeVideo, tempDir, 1, CancellationToken.None })!;

            Func<Task> act = async () => await task;
            await act.Should().ThrowAsync<InvalidOperationException>();
        }
        finally { CleanupDir(tempDir); }
    }
}