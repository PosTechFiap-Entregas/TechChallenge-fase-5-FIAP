using FiapX.Domain.Entities;
using FiapX.Domain.Enums;
using FluentAssertions;

namespace FiapX.Domain.Tests.Entities;

public class VideoTests
{
    [Fact]
    public void Constructor_WithValidData_ShouldCreateVideo()
    {
        // Arrange & Act
        var userId = Guid.NewGuid();
        var video = new Video(userId, "video.mp4", "/storage/video.mp4", 1024);

        // Assert
        video.Should().NotBeNull();
        video.UserId.Should().Be(userId);
        video.OriginalFileName.Should().Be("video.mp4");
        video.StoragePath.Should().Be("/storage/video.mp4");
        video.FileSizeBytes.Should().Be(1024);
        video.Status.Should().Be(VideoStatus.Uploaded);
        video.UploadedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_WithInvalidFileName_ShouldThrowException(string fileName)
    {
        // Arrange & Act
        var act = () => new Video(Guid.NewGuid(), fileName, "/storage/path", 1024);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*File name cannot be empty*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_WithInvalidStoragePath_ShouldThrowException(string path)
    {
        // Arrange & Act
        var act = () => new Video(Guid.NewGuid(), "file.mp4", path, 1024);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Storage path cannot be empty*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Constructor_WithInvalidFileSize_ShouldThrowException(long fileSize)
    {
        // Arrange & Act
        var act = () => new Video(Guid.NewGuid(), "file.mp4", "/storage/path", fileSize);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*File size must be greater than zero*");
    }

    [Fact]
    public void Constructor_WithFileSizeExceeding2GB_ShouldThrowException()
    {
        // Arrange
        var fileSize = 3L * 1024 * 1024 * 1024; // 3GB

        // Act
        var act = () => new Video(Guid.NewGuid(), "file.mp4", "/storage/path", fileSize);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*File size exceeds maximum of 2GB*");
    }

    [Fact]
    public void MarkAsQueued_FromUploaded_ShouldChangeStatus()
    {
        // Arrange
        var video = new Video(Guid.NewGuid(), "video.mp4", "/storage/video.mp4", 1024);
        video.Status.Should().Be(VideoStatus.Uploaded);

        // Act
        video.MarkAsQueued();

        // Assert
        video.Status.Should().Be(VideoStatus.Queued);
    }

    [Fact]
    public void MarkAsQueued_FromNonUploadedStatus_ShouldThrowException()
    {
        // Arrange
        var video = new Video(Guid.NewGuid(), "video.mp4", "/storage/video.mp4", 1024);
        video.MarkAsQueued();
        video.Status.Should().Be(VideoStatus.Queued);

        // Act
        var act = () => video.MarkAsQueued();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot queue video in status*");
    }

    [Fact]
    public void StartProcessing_FromQueued_ShouldChangeStatus()
    {
        // Arrange
        var video = new Video(Guid.NewGuid(), "video.mp4", "/storage/video.mp4", 1024);
        video.MarkAsQueued();

        // Act
        video.StartProcessing();

        // Assert
        video.Status.Should().Be(VideoStatus.Processing);
    }

    [Fact]
    public void StartProcessing_FromNonQueuedStatus_ShouldThrowException()
    {
        // Arrange
        var video = new Video(Guid.NewGuid(), "video.mp4", "/storage/video.mp4", 1024);
        // Status: Uploaded

        // Act
        var act = () => video.StartProcessing();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot start processing video in status*");
    }

    [Fact]
    public void CompleteProcessing_FromProcessing_ShouldUpdateAllFields()
    {
        // Arrange
        var video = new Video(Guid.NewGuid(), "video.mp4", "/storage/video.mp4", 1024);
        video.MarkAsQueued();
        video.StartProcessing();
        var duration = TimeSpan.FromSeconds(10);

        // Act
        video.CompleteProcessing("/storage/frames.zip", 100, duration);

        // Assert
        video.Status.Should().Be(VideoStatus.Completed);
        video.ZipPath.Should().Be("/storage/frames.zip");
        video.FrameCount.Should().Be(100);
        video.ProcessingDuration.Should().Be(duration);
        video.ProcessedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        video.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void CompleteProcessing_FromNonProcessingStatus_ShouldThrowException()
    {
        // Arrange
        var video = new Video(Guid.NewGuid(), "video.mp4", "/storage/video.mp4", 1024);
        // Status: Uploaded

        // Act
        var act = () => video.CompleteProcessing("/storage/frames.zip", 100, TimeSpan.FromSeconds(10));

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot complete video not in processing status*");
    }

    [Fact]
    public void FailProcessing_FromProcessing_ShouldUpdateStatus()
    {
        // Arrange
        var video = new Video(Guid.NewGuid(), "video.mp4", "/storage/video.mp4", 1024);
        video.MarkAsQueued();
        video.StartProcessing();

        // Act
        video.FailProcessing("FFmpeg error");

        // Assert
        video.Status.Should().Be(VideoStatus.Failed);
        video.ErrorMessage.Should().Be("FFmpeg error");
        video.ProcessedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void FailProcessing_FromNonProcessingStatus_ShouldThrowException()
    {
        // Arrange
        var video = new Video(Guid.NewGuid(), "video.mp4", "/storage/video.mp4", 1024);
        // Status: Uploaded

        // Act
        var act = () => video.FailProcessing("Error");

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot fail video not in processing status*");
    }

    [Fact]
    public void RetryProcessing_FromFailed_ShouldResetToQueued()
    {
        // Arrange
        var video = new Video(Guid.NewGuid(), "video.mp4", "/storage/video.mp4", 1024);
        video.MarkAsQueued();
        video.StartProcessing();
        video.FailProcessing("Error");
        video.Status.Should().Be(VideoStatus.Failed);

        // Act
        video.RetryProcessing();

        // Assert
        video.Status.Should().Be(VideoStatus.Queued);
        video.ErrorMessage.Should().BeNull();
        video.ProcessedAt.Should().BeNull();
        video.FrameCount.Should().BeNull();
        video.ZipPath.Should().BeNull();
        video.ProcessingDuration.Should().BeNull();
    }

    [Fact]
    public void RetryProcessing_FromNonFailedStatus_ShouldThrowException()
    {
        // Arrange
        var video = new Video(Guid.NewGuid(), "video.mp4", "/storage/video.mp4", 1024);
        // Status: Uploaded

        // Act
        var act = () => video.RetryProcessing();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot retry video not in failed status*");
    }

    [Fact]
    public void IsProcessed_WithCompletedVideo_ShouldReturnTrue()
    {
        // Arrange
        var video = new Video(Guid.NewGuid(), "video.mp4", "/storage/video.mp4", 1024);
        video.MarkAsQueued();
        video.StartProcessing();
        video.CompleteProcessing("/storage/frames.zip", 100, TimeSpan.FromSeconds(10));

        // Act & Assert
        video.IsProcessed().Should().BeTrue();
        video.IsFailed().Should().BeFalse();
        video.IsProcessing().Should().BeFalse();
    }

    [Fact]
    public void IsFailed_WithFailedVideo_ShouldReturnTrue()
    {
        // Arrange
        var video = new Video(Guid.NewGuid(), "video.mp4", "/storage/video.mp4", 1024);
        video.MarkAsQueued();
        video.StartProcessing();
        video.FailProcessing("Error");

        // Act & Assert
        video.IsFailed().Should().BeTrue();
        video.IsProcessed().Should().BeFalse();
        video.IsProcessing().Should().BeFalse();
    }

    [Fact]
    public void CanDownload_WithCompletedVideo_ShouldReturnTrue()
    {
        // Arrange
        var video = new Video(Guid.NewGuid(), "video.mp4", "/storage/video.mp4", 1024);
        video.MarkAsQueued();
        video.StartProcessing();
        video.CompleteProcessing("/storage/frames.zip", 100, TimeSpan.FromSeconds(10));

        // Act & Assert
        video.CanDownload().Should().BeTrue();
    }

    [Fact]
    public void CanDownload_WithNonCompletedVideo_ShouldReturnFalse()
    {
        // Arrange
        var video = new Video(Guid.NewGuid(), "video.mp4", "/storage/video.mp4", 1024);

        // Act & Assert
        video.CanDownload().Should().BeFalse();
    }

    [Fact]
    public void Constructor_WithFileNameTooLong_ShouldThrowException()
    {
        // Arrange
        var longFileName = new string('a', 256) + ".mp4"; // > 255 chars

        // Act
        var act = () => new Video(Guid.NewGuid(), longFileName, "/storage/path", 1024);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*File name too long*");
    }
}