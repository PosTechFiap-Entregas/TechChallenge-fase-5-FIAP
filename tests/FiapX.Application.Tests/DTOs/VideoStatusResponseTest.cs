using FiapX.Application.DTOs;
using FluentAssertions;

namespace FiapX.Application.Tests.DTOs;

public class VideoStatusResponseTests
{
    [Fact]
    public void VideoStatusResponse_DefaultValues_ShouldBeInitializedCorrectly()
    {
        var response = new VideoStatusResponse();

        response.VideoId.Should().Be(Guid.Empty);
        response.OriginalFileName.Should().Be(string.Empty);
        response.Status.Should().Be(string.Empty);
        response.StatusDescription.Should().Be(string.Empty);
        response.UploadedAt.Should().Be(default(DateTime));
        response.ProcessedAt.Should().BeNull();
        response.FrameCount.Should().BeNull();
        response.ErrorMessage.Should().BeNull();
        response.ProcessingDurationSeconds.Should().BeNull();
        response.CanDownload.Should().BeFalse();
    }

    [Fact]
    public void VideoStatusResponse_WithAllPropertiesSet_ShouldReturnCorrectValues()
    {
        var videoId = Guid.NewGuid();
        var uploadedAt = DateTime.UtcNow.AddMinutes(-10);
        var processedAt = DateTime.UtcNow;

        var response = new VideoStatusResponse
        {
            VideoId = videoId,
            OriginalFileName = "recording.mp4",
            Status = "Completed",
            StatusDescription = "All frames extracted.",
            UploadedAt = uploadedAt,
            ProcessedAt = processedAt,
            FrameCount = 480,
            ErrorMessage = null,
            ProcessingDurationSeconds = 12.5,
            CanDownload = true
        };

        response.VideoId.Should().Be(videoId);
        response.OriginalFileName.Should().Be("recording.mp4");
        response.Status.Should().Be("Completed");
        response.StatusDescription.Should().Be("All frames extracted.");
        response.UploadedAt.Should().Be(uploadedAt);
        response.ProcessedAt.Should().Be(processedAt);
        response.FrameCount.Should().Be(480);
        response.ErrorMessage.Should().BeNull();
        response.ProcessingDurationSeconds.Should().BeApproximately(12.5, 0.001);
        response.CanDownload.Should().BeTrue();
    }

    [Fact]
    public void VideoStatusResponse_ProcessedAt_ShouldAcceptNull()
    {
        var response = new VideoStatusResponse { ProcessedAt = null };

        response.ProcessedAt.Should().BeNull();
    }

    [Fact]
    public void VideoStatusResponse_ProcessedAt_ShouldAcceptValue()
    {
        var dt = DateTime.UtcNow;

        var response = new VideoStatusResponse { ProcessedAt = dt };

        response.ProcessedAt.Should().Be(dt);
    }

    [Fact]
    public void VideoStatusResponse_FrameCount_ShouldAcceptNull()
    {
        var response = new VideoStatusResponse { FrameCount = null };

        response.FrameCount.Should().BeNull();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(60)]
    [InlineData(7200)]
    [InlineData(int.MaxValue)]
    public void VideoStatusResponse_FrameCount_ShouldAcceptPositiveValues(int count)
    {
        var response = new VideoStatusResponse { FrameCount = count };

        response.FrameCount.Should().Be(count);
    }

    [Fact]
    public void VideoStatusResponse_ErrorMessage_ShouldAcceptNull()
    {
        var response = new VideoStatusResponse { ErrorMessage = null };

        response.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void VideoStatusResponse_ErrorMessage_ShouldAcceptValue()
    {
        var response = new VideoStatusResponse { ErrorMessage = "FFmpeg process failed with exit code 1." };

        response.ErrorMessage.Should().Be("FFmpeg process failed with exit code 1.");
    }

    [Fact]
    public void VideoStatusResponse_ProcessingDurationSeconds_ShouldAcceptNull()
    {
        var response = new VideoStatusResponse { ProcessingDurationSeconds = null };

        response.ProcessingDurationSeconds.Should().BeNull();
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(0.5)]
    [InlineData(60.0)]
    [InlineData(3600.0)]
    public void VideoStatusResponse_ProcessingDurationSeconds_ShouldAcceptValidValues(double duration)
    {
        var response = new VideoStatusResponse { ProcessingDurationSeconds = duration };

        response.ProcessingDurationSeconds.Should().BeApproximately(duration, 0.0001);
    }

    [Fact]
    public void VideoStatusResponse_WhenStatusCompleted_CanDownloadShouldBeTrue()
    {
        var response = new VideoStatusResponse { Status = "Completed", CanDownload = true };

        response.CanDownload.Should().BeTrue();
    }

    [Fact]
    public void VideoStatusResponse_WhenStatusFailed_CanDownloadShouldBeFalse()
    {
        var response = new VideoStatusResponse { Status = "Failed", CanDownload = false };

        response.CanDownload.Should().BeFalse();
    }

    [Fact]
    public void VideoStatusResponse_WhenStatusPending_CanDownloadShouldBeFalse()
    {
        var response = new VideoStatusResponse { Status = "Pending", CanDownload = false };

        response.CanDownload.Should().BeFalse();
    }

    [Fact]
    public void VideoStatusResponse_FailedState_ShouldHaveErrorMessageAndNoFrames()
    {
        var response = new VideoStatusResponse
        {
            Status = "Failed",
            StatusDescription = "Processing error.",
            ErrorMessage = "Unsupported codec.",
            FrameCount = null,
            ProcessedAt = null,
            ProcessingDurationSeconds = null,
            CanDownload = false
        };

        response.Status.Should().Be("Failed");
        response.ErrorMessage.Should().NotBeNullOrEmpty();
        response.FrameCount.Should().BeNull();
        response.CanDownload.Should().BeFalse();
        response.ProcessedAt.Should().BeNull();
    }

    [Fact]
    public void VideoStatusResponse_CompletedState_ShouldHaveFramesAndDownloadEnabled()
    {
        var response = new VideoStatusResponse
        {
            Status = "Completed",
            FrameCount = 240,
            ProcessedAt = DateTime.UtcNow,
            ProcessingDurationSeconds = 8.3,
            ErrorMessage = null,
            CanDownload = true
        };

        response.Status.Should().Be("Completed");
        response.FrameCount.Should().BePositive();
        response.ProcessedAt.Should().NotBeNull();
        response.ProcessingDurationSeconds.Should().BePositive();
        response.ErrorMessage.Should().BeNull();
        response.CanDownload.Should().BeTrue();
    }

    [Fact]
    public void VideoStatusResponse_TwoInstancesWithSameValues_ShouldBeEqual()
    {
        var videoId = Guid.NewGuid();
        var date = new DateTime(2024, 3, 10, 12, 0, 0, DateTimeKind.Utc);

        var r1 = new VideoStatusResponse { VideoId = videoId, Status = "Completed", UploadedAt = date, FrameCount = 120, CanDownload = true };
        var r2 = new VideoStatusResponse { VideoId = videoId, Status = "Completed", UploadedAt = date, FrameCount = 120, CanDownload = true };

        r1.Should().Be(r2);
        (r1 == r2).Should().BeTrue();
    }

    [Fact]
    public void VideoStatusResponse_TwoInstancesWithDifferentErrorMessage_ShouldNotBeEqual()
    {
        var id = Guid.NewGuid();
        var r1 = new VideoStatusResponse { VideoId = id, ErrorMessage = "Error A" };
        var r2 = new VideoStatusResponse { VideoId = id, ErrorMessage = "Error B" };

        r1.Should().NotBe(r2);
    }

    [Fact]
    public void VideoStatusResponse_TwoInstancesWithDifferentProcessingDuration_ShouldNotBeEqual()
    {
        var id = Guid.NewGuid();
        var r1 = new VideoStatusResponse { VideoId = id, ProcessingDurationSeconds = 5.0 };
        var r2 = new VideoStatusResponse { VideoId = id, ProcessingDurationSeconds = 10.0 };

        r1.Should().NotBe(r2);
    }

    [Fact]
    public void VideoStatusResponse_WithExpression_ShouldUpdateStatusAndPreserveOtherFields()
    {
        var videoId = Guid.NewGuid();
        var original = new VideoStatusResponse { VideoId = videoId, Status = "Processing", CanDownload = false };

        var updated = original with { Status = "Completed", CanDownload = true };

        updated.Status.Should().Be("Completed");
        updated.CanDownload.Should().BeTrue();
        updated.VideoId.Should().Be(videoId);
        original.Status.Should().Be("Processing");
        original.CanDownload.Should().BeFalse();
    }

    [Fact]
    public void VideoStatusResponse_TwoEqualInstances_ShouldHaveSameHashCode()
    {
        var id = Guid.NewGuid();
        var date = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var r1 = new VideoStatusResponse { VideoId = id, UploadedAt = date, FrameCount = 60 };
        var r2 = new VideoStatusResponse { VideoId = id, UploadedAt = date, FrameCount = 60 };

        r1.GetHashCode().Should().Be(r2.GetHashCode());
    }

    [Fact]
    public void VideoStatusResponse_ToString_ShouldContainTypeName()
    {
        var response = new VideoStatusResponse { OriginalFileName = "test.mp4", Status = "Processing" };

        var result = response.ToString();

        result.Should().Contain("VideoStatusResponse");
        result.Should().Contain("test.mp4");
    }
}
