using FiapX.Application.DTOs;
using FluentAssertions;

namespace FiapX.Application.Tests.DTOs;

public class VideoListResponseTests
{
    [Fact]
    public void VideoListResponse_DefaultValues_ShouldBeInitializedCorrectly()
    {
        var response = new VideoListResponse();

        response.VideoId.Should().Be(Guid.Empty);
        response.OriginalFileName.Should().Be(string.Empty);
        response.FileSizeMB.Should().Be(0.0);
        response.Status.Should().Be(string.Empty);
        response.StatusDescription.Should().Be(string.Empty);
        response.UploadedAt.Should().Be(default(DateTime));
        response.ProcessedAt.Should().BeNull();
        response.FrameCount.Should().BeNull();
        response.CanDownload.Should().BeFalse();
    }

    [Fact]
    public void VideoListResponse_WithAllPropertiesSet_ShouldReturnCorrectValues()
    {
        var videoId = Guid.NewGuid();
        var uploadedAt = DateTime.UtcNow.AddHours(-2);
        var processedAt = DateTime.UtcNow;

        var response = new VideoListResponse
        {
            VideoId = videoId,
            OriginalFileName = "myvideo.mp4",
            FileSizeMB = 15.7,
            Status = "Completed",
            StatusDescription = "Processing completed successfully.",
            UploadedAt = uploadedAt,
            ProcessedAt = processedAt,
            FrameCount = 300,
            CanDownload = true
        };

        response.VideoId.Should().Be(videoId);
        response.OriginalFileName.Should().Be("myvideo.mp4");
        response.FileSizeMB.Should().BeApproximately(15.7, 0.001);
        response.Status.Should().Be("Completed");
        response.StatusDescription.Should().Be("Processing completed successfully.");
        response.UploadedAt.Should().Be(uploadedAt);
        response.ProcessedAt.Should().Be(processedAt);
        response.FrameCount.Should().Be(300);
        response.CanDownload.Should().BeTrue();
    }

    [Fact]
    public void VideoListResponse_ProcessedAt_ShouldAcceptNull()
    {
        var response = new VideoListResponse { ProcessedAt = null };

        response.ProcessedAt.Should().BeNull();
    }

    [Fact]
    public void VideoListResponse_ProcessedAt_ShouldAcceptValue()
    {
        var processed = DateTime.UtcNow;

        var response = new VideoListResponse { ProcessedAt = processed };

        response.ProcessedAt.Should().Be(processed);
    }

    [Fact]
    public void VideoListResponse_FrameCount_ShouldAcceptNull()
    {
        var response = new VideoListResponse { FrameCount = null };

        response.FrameCount.Should().BeNull();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(int.MaxValue)]
    public void VideoListResponse_FrameCount_ShouldAcceptPositiveValues(int frameCount)
    {
        var response = new VideoListResponse { FrameCount = frameCount };

        response.FrameCount.Should().Be(frameCount);
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(0.1)]
    [InlineData(100.0)]
    [InlineData(1024.0)]
    public void VideoListResponse_FileSizeMB_ShouldAcceptValidValues(double sizeMB)
    {
        var response = new VideoListResponse { FileSizeMB = sizeMB };

        response.FileSizeMB.Should().BeApproximately(sizeMB, 0.0001);
    }

    [Fact]
    public void VideoListResponse_CanDownload_WhenTrue_ShouldBeTrue()
    {
        var response = new VideoListResponse { Status = "Completed", CanDownload = true };

        response.CanDownload.Should().BeTrue();
    }

    [Fact]
    public void VideoListResponse_CanDownload_WhenFalse_ShouldBeFalse()
    {
        var response = new VideoListResponse { Status = "Processing", CanDownload = false };

        response.CanDownload.Should().BeFalse();
    }

    [Fact]
    public void VideoListResponse_TwoInstancesWithSameValues_ShouldBeEqual()
    {
        var videoId = Guid.NewGuid();
        var uploadedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var r1 = new VideoListResponse { VideoId = videoId, Status = "Completed", UploadedAt = uploadedAt, FrameCount = 150, CanDownload = true };
        var r2 = new VideoListResponse { VideoId = videoId, Status = "Completed", UploadedAt = uploadedAt, FrameCount = 150, CanDownload = true };

        r1.Should().Be(r2);
        (r1 == r2).Should().BeTrue();
    }

    [Fact]
    public void VideoListResponse_TwoInstancesWithDifferentFrameCount_ShouldNotBeEqual()
    {
        var id = Guid.NewGuid();
        var r1 = new VideoListResponse { VideoId = id, FrameCount = 100 };
        var r2 = new VideoListResponse { VideoId = id, FrameCount = 200 };

        r1.Should().NotBe(r2);
    }

    [Fact]
    public void VideoListResponse_TwoInstancesWithDifferentCanDownload_ShouldNotBeEqual()
    {
        var id = Guid.NewGuid();
        var r1 = new VideoListResponse { VideoId = id, CanDownload = true };
        var r2 = new VideoListResponse { VideoId = id, CanDownload = false };

        r1.Should().NotBe(r2);
    }

    [Fact]
    public void VideoListResponse_WithExpression_ShouldUpdateCanDownloadAndPreserveOtherFields()
    {
        var videoId = Guid.NewGuid();
        var original = new VideoListResponse { VideoId = videoId, Status = "Completed", CanDownload = false };

        var updated = original with { CanDownload = true };

        updated.CanDownload.Should().BeTrue();
        updated.VideoId.Should().Be(videoId);
        updated.Status.Should().Be("Completed");
        original.CanDownload.Should().BeFalse();
    }

    [Fact]
    public void VideoListResponse_TwoEqualInstances_ShouldHaveSameHashCode()
    {
        var id = Guid.NewGuid();
        var date = new DateTime(2024, 6, 15, 0, 0, 0, DateTimeKind.Utc);

        var r1 = new VideoListResponse { VideoId = id, UploadedAt = date, CanDownload = true };
        var r2 = new VideoListResponse { VideoId = id, UploadedAt = date, CanDownload = true };

        r1.GetHashCode().Should().Be(r2.GetHashCode());
    }

    [Fact]
    public void VideoListResponse_ToString_ShouldContainTypeName()
    {
        var response = new VideoListResponse { OriginalFileName = "clip.mp4", Status = "Pending" };

        var result = response.ToString();

        result.Should().Contain("VideoListResponse");
        result.Should().Contain("clip.mp4");
    }
}
