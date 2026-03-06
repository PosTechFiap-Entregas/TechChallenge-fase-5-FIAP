using FiapX.Application.DTOs;
using FluentAssertions;

namespace FiapX.Application.Tests.DTOs;

public class UploadVideoResponseTests
{
    [Fact]
    public void UploadVideoResponse_DefaultValues_ShouldBeInitializedCorrectly()
    {
        var response = new UploadVideoResponse();

        response.VideoId.Should().Be(Guid.Empty);
        response.OriginalFileName.Should().Be(string.Empty);
        response.Status.Should().Be(string.Empty);
        response.Message.Should().Be(string.Empty);
    }

    [Fact]
    public void UploadVideoResponse_WithAllPropertiesSet_ShouldReturnCorrectValues()
    {
        var videoId = Guid.NewGuid();

        var response = new UploadVideoResponse
        {
            VideoId = videoId,
            OriginalFileName = "video.mp4",
            Status = "Pending",
            Message = "Upload received successfully."
        };

        response.VideoId.Should().Be(videoId);
        response.OriginalFileName.Should().Be("video.mp4");
        response.Status.Should().Be("Pending");
        response.Message.Should().Be("Upload received successfully.");
    }

    [Fact]
    public void UploadVideoResponse_VideoId_ShouldAcceptValidGuid()
    {
        var guid = Guid.NewGuid();

        var response = new UploadVideoResponse { VideoId = guid };

        response.VideoId.Should().NotBe(Guid.Empty);
        response.VideoId.Should().Be(guid);
    }

    [Theory]
    [InlineData("Pending")]
    [InlineData("Processing")]
    [InlineData("Completed")]
    [InlineData("Failed")]
    public void UploadVideoResponse_Status_ShouldAcceptCommonStatusValues(string status)
    {
        var response = new UploadVideoResponse { Status = status };

        response.Status.Should().Be(status);
    }

    [Fact]
    public void UploadVideoResponse_TwoInstancesWithSameValues_ShouldBeEqual()
    {
        var videoId = Guid.NewGuid();
        var r1 = new UploadVideoResponse { VideoId = videoId, OriginalFileName = "v.mp4", Status = "Pending", Message = "OK" };
        var r2 = new UploadVideoResponse { VideoId = videoId, OriginalFileName = "v.mp4", Status = "Pending", Message = "OK" };

        r1.Should().Be(r2);
        (r1 == r2).Should().BeTrue();
    }

    [Fact]
    public void UploadVideoResponse_TwoInstancesWithDifferentVideoId_ShouldNotBeEqual()
    {
        var r1 = new UploadVideoResponse { VideoId = Guid.NewGuid() };
        var r2 = new UploadVideoResponse { VideoId = Guid.NewGuid() };

        r1.Should().NotBe(r2);
    }

    [Fact]
    public void UploadVideoResponse_TwoInstancesWithDifferentStatus_ShouldNotBeEqual()
    {
        var id = Guid.NewGuid();
        var r1 = new UploadVideoResponse { VideoId = id, Status = "Pending" };
        var r2 = new UploadVideoResponse { VideoId = id, Status = "Completed" };

        r1.Should().NotBe(r2);
    }

    [Fact]
    public void UploadVideoResponse_WithExpression_ShouldUpdateStatusAndPreserveOtherFields()
    {
        var videoId = Guid.NewGuid();
        var original = new UploadVideoResponse { VideoId = videoId, OriginalFileName = "v.mp4", Status = "Pending", Message = "Queued" };

        var updated = original with { Status = "Processing" };

        updated.Status.Should().Be("Processing");
        updated.VideoId.Should().Be(videoId);
        updated.OriginalFileName.Should().Be("v.mp4");
        original.Status.Should().Be("Pending");
    }

    [Fact]
    public void UploadVideoResponse_TwoEqualInstances_ShouldHaveSameHashCode()
    {
        var id = Guid.NewGuid();
        var r1 = new UploadVideoResponse { VideoId = id, Status = "Pending" };
        var r2 = new UploadVideoResponse { VideoId = id, Status = "Pending" };

        r1.GetHashCode().Should().Be(r2.GetHashCode());
    }

    [Fact]
    public void UploadVideoResponse_ToString_ShouldContainTypeName()
    {
        var response = new UploadVideoResponse { Status = "Pending" };

        var result = response.ToString();

        result.Should().Contain("UploadVideoResponse");
        result.Should().Contain("Pending");
    }
}
