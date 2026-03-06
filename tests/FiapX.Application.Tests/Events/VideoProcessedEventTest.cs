using FiapX.Application.Events;
using FluentAssertions;

namespace FiapX.Application.Tests.Events;

public class VideoProcessedEventTests
{
    [Fact]
    public void VideoProcessedEvent_DefaultValues_ShouldBeInitializedCorrectly()
    {
        var evt = new VideoProcessedEvent();

        evt.VideoId.Should().Be(Guid.Empty);
        evt.UserId.Should().Be(Guid.Empty);
        evt.Success.Should().BeFalse();
        evt.FrameCount.Should().BeNull();
        evt.ZipPath.Should().BeNull();
        evt.ErrorMessage.Should().BeNull();
        evt.ProcessingDuration.Should().BeNull();
        evt.ProcessedAt.Should().Be(default(DateTime));
    }

    [Fact]
    public void VideoProcessedEvent_WithAllPropertiesSet_ShouldReturnCorrectValues()
    {
        var videoId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var processedAt = DateTime.UtcNow;
        var duration = TimeSpan.FromSeconds(15.5);

        var evt = new VideoProcessedEvent
        {
            VideoId = videoId,
            UserId = userId,
            Success = true,
            FrameCount = 360,
            ZipPath = "/storage/zips/frames.zip",
            ErrorMessage = null,
            ProcessingDuration = duration,
            ProcessedAt = processedAt
        };

        evt.VideoId.Should().Be(videoId);
        evt.UserId.Should().Be(userId);
        evt.Success.Should().BeTrue();
        evt.FrameCount.Should().Be(360);
        evt.ZipPath.Should().Be("/storage/zips/frames.zip");
        evt.ErrorMessage.Should().BeNull();
        evt.ProcessingDuration.Should().Be(duration);
        evt.ProcessedAt.Should().Be(processedAt);
    }

    [Fact]
    public void VideoProcessedEvent_SuccessState_ShouldHaveFrameCountAndZipPath()
    {
        var evt = new VideoProcessedEvent
        {
            Success = true,
            FrameCount = 240,
            ZipPath = "/storage/zips/output.zip",
            ErrorMessage = null,
            ProcessingDuration = TimeSpan.FromSeconds(8),
            ProcessedAt = DateTime.UtcNow
        };

        evt.Success.Should().BeTrue();
        evt.FrameCount.Should().BePositive();
        evt.ZipPath.Should().NotBeNullOrEmpty();
        evt.ErrorMessage.Should().BeNull();
        evt.ProcessingDuration.Should().NotBeNull();
        evt.ProcessingDuration!.Value.TotalSeconds.Should().BePositive();
    }

    [Fact]
    public void VideoProcessedEvent_FailureState_ShouldHaveErrorMessageAndNoZip()
    {
        var evt = new VideoProcessedEvent
        {
            Success = false,
            FrameCount = null,
            ZipPath = null,
            ErrorMessage = "FFmpeg process exited with code 1.",
            ProcessingDuration = null,
            ProcessedAt = DateTime.UtcNow
        };

        evt.Success.Should().BeFalse();
        evt.FrameCount.Should().BeNull();
        evt.ZipPath.Should().BeNull();
        evt.ErrorMessage.Should().NotBeNullOrEmpty();
        evt.ProcessingDuration.Should().BeNull();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void VideoProcessedEvent_Success_ShouldAcceptBothValues(bool success)
    {
        var evt = new VideoProcessedEvent { Success = success };

        evt.Success.Should().Be(success);
    }

    [Fact]
    public void VideoProcessedEvent_FrameCount_ShouldAcceptNull()
    {
        var evt = new VideoProcessedEvent { FrameCount = null };

        evt.FrameCount.Should().BeNull();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(60)]
    [InlineData(3600)]
    [InlineData(int.MaxValue)]
    public void VideoProcessedEvent_FrameCount_ShouldAcceptPositiveValues(int count)
    {
        var evt = new VideoProcessedEvent { FrameCount = count };

        evt.FrameCount.Should().Be(count);
    }

    [Fact]
    public void VideoProcessedEvent_ZipPath_ShouldAcceptNull()
    {
        var evt = new VideoProcessedEvent { ZipPath = null };

        evt.ZipPath.Should().BeNull();
    }

    [Theory]
    [InlineData("/storage/zips/frames.zip")]
    [InlineData("C:\\Output\\video_frames.zip")]
    [InlineData("output/2024/06/frames.zip")]
    public void VideoProcessedEvent_ZipPath_ShouldAcceptValidPaths(string path)
    {
        var evt = new VideoProcessedEvent { ZipPath = path };

        evt.ZipPath.Should().Be(path);
    }

    [Fact]
    public void VideoProcessedEvent_ErrorMessage_ShouldAcceptNull()
    {
        var evt = new VideoProcessedEvent { ErrorMessage = null };

        evt.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void VideoProcessedEvent_ErrorMessage_ShouldAcceptDescriptiveMessage()
    {
        var evt = new VideoProcessedEvent { ErrorMessage = "Unsupported video codec: H.265." };

        evt.ErrorMessage.Should().Be("Unsupported video codec: H.265.");
    }

    [Fact]
    public void VideoProcessedEvent_ErrorMessage_ShouldAcceptEmptyString()
    {
        var evt = new VideoProcessedEvent { ErrorMessage = string.Empty };

        evt.ErrorMessage.Should().BeEmpty();
    }

    [Fact]
    public void VideoProcessedEvent_ProcessingDuration_ShouldAcceptNull()
    {
        var evt = new VideoProcessedEvent { ProcessingDuration = null };

        evt.ProcessingDuration.Should().BeNull();
    }

    [Theory]
    [MemberData(nameof(ValidDurations))]
    public void VideoProcessedEvent_ProcessingDuration_ShouldAcceptValidTimeSpans(TimeSpan duration)
    {
        var evt = new VideoProcessedEvent { ProcessingDuration = duration };

        evt.ProcessingDuration.Should().Be(duration);
    }

    public static IEnumerable<object[]> ValidDurations =>
    [
        [TimeSpan.Zero],
        [TimeSpan.FromMilliseconds(500)],
        [TimeSpan.FromSeconds(1)],
        [TimeSpan.FromSeconds(30)],
        [TimeSpan.FromMinutes(5)],
        [TimeSpan.FromHours(1)]
    ];

    [Fact]
    public void VideoProcessedEvent_ProcessingDuration_TotalSecondsShouldBeAccessible()
    {
        var duration = TimeSpan.FromSeconds(42.5);

        var evt = new VideoProcessedEvent { ProcessingDuration = duration };

        evt.ProcessingDuration!.Value.TotalSeconds.Should().BeApproximately(42.5, 0.001);
    }

    [Fact]
    public void VideoProcessedEvent_ProcessedAt_ShouldAcceptUtcNow()
    {
        var now = DateTime.UtcNow;

        var evt = new VideoProcessedEvent { ProcessedAt = now };

        evt.ProcessedAt.Should().BeCloseTo(now, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void VideoProcessedEvent_ProcessedAt_ShouldAcceptPastDate()
    {
        var past = new DateTime(2022, 3, 15, 8, 0, 0, DateTimeKind.Utc);

        var evt = new VideoProcessedEvent { ProcessedAt = past };

        evt.ProcessedAt.Should().BeBefore(DateTime.UtcNow);
    }

    [Fact]
    public void VideoProcessedEvent_TwoInstancesWithSameValues_ShouldBeEqual()
    {
        var videoId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var processedAt = new DateTime(2024, 7, 10, 10, 0, 0, DateTimeKind.Utc);
        var duration = TimeSpan.FromSeconds(10);

        var e1 = new VideoProcessedEvent
        {
            VideoId = videoId,
            UserId = userId,
            Success = true,
            FrameCount = 120,
            ZipPath = "/zip/frames.zip",
            ErrorMessage = null,
            ProcessingDuration = duration,
            ProcessedAt = processedAt
        };

        var e2 = new VideoProcessedEvent
        {
            VideoId = videoId,
            UserId = userId,
            Success = true,
            FrameCount = 120,
            ZipPath = "/zip/frames.zip",
            ErrorMessage = null,
            ProcessingDuration = duration,
            ProcessedAt = processedAt
        };

        e1.Should().Be(e2);
        (e1 == e2).Should().BeTrue();
    }

    [Fact]
    public void VideoProcessedEvent_TwoInstancesWithDifferentSuccess_ShouldNotBeEqual()
    {
        var id = Guid.NewGuid();
        var e1 = new VideoProcessedEvent { VideoId = id, Success = true };
        var e2 = new VideoProcessedEvent { VideoId = id, Success = false };

        e1.Should().NotBe(e2);
        (e1 != e2).Should().BeTrue();
    }

    [Fact]
    public void VideoProcessedEvent_TwoInstancesWithDifferentFrameCount_ShouldNotBeEqual()
    {
        var id = Guid.NewGuid();
        var e1 = new VideoProcessedEvent { VideoId = id, FrameCount = 100 };
        var e2 = new VideoProcessedEvent { VideoId = id, FrameCount = 200 };

        e1.Should().NotBe(e2);
    }

    [Fact]
    public void VideoProcessedEvent_TwoInstancesWithDifferentZipPath_ShouldNotBeEqual()
    {
        var id = Guid.NewGuid();
        var e1 = new VideoProcessedEvent { VideoId = id, ZipPath = "/a.zip" };
        var e2 = new VideoProcessedEvent { VideoId = id, ZipPath = "/b.zip" };

        e1.Should().NotBe(e2);
    }

    [Fact]
    public void VideoProcessedEvent_TwoInstancesWithDifferentErrorMessage_ShouldNotBeEqual()
    {
        var id = Guid.NewGuid();
        var e1 = new VideoProcessedEvent { VideoId = id, ErrorMessage = "Error A" };
        var e2 = new VideoProcessedEvent { VideoId = id, ErrorMessage = "Error B" };

        e1.Should().NotBe(e2);
    }

    [Fact]
    public void VideoProcessedEvent_TwoInstancesWithDifferentProcessingDuration_ShouldNotBeEqual()
    {
        var id = Guid.NewGuid();
        var e1 = new VideoProcessedEvent { VideoId = id, ProcessingDuration = TimeSpan.FromSeconds(5) };
        var e2 = new VideoProcessedEvent { VideoId = id, ProcessingDuration = TimeSpan.FromSeconds(10) };

        e1.Should().NotBe(e2);
    }

    [Fact]
    public void VideoProcessedEvent_TwoInstancesWithDifferentProcessedAt_ShouldNotBeEqual()
    {
        var id = Guid.NewGuid();
        var e1 = new VideoProcessedEvent { VideoId = id, ProcessedAt = DateTime.UtcNow };
        var e2 = new VideoProcessedEvent { VideoId = id, ProcessedAt = DateTime.UtcNow.AddMinutes(-1) };

        e1.Should().NotBe(e2);
    }

    [Fact]
    public void VideoProcessedEvent_WithExpression_ShouldUpdateSuccessAndPreserveOtherFields()
    {
        var videoId = Guid.NewGuid();
        var original = new VideoProcessedEvent
        {
            VideoId = videoId,
            Success = false,
            ErrorMessage = "Timeout",
            FrameCount = null
        };

        var updated = original with { Success = true, ErrorMessage = null, FrameCount = 180 };

        updated.Success.Should().BeTrue();
        updated.ErrorMessage.Should().BeNull();
        updated.FrameCount.Should().Be(180);
        updated.VideoId.Should().Be(videoId);

        original.Success.Should().BeFalse();
        original.ErrorMessage.Should().Be("Timeout");
        original.FrameCount.Should().BeNull();
    }

    [Fact]
    public void VideoProcessedEvent_WithExpression_ShouldUpdateProcessingDuration()
    {
        var original = new VideoProcessedEvent { ProcessingDuration = TimeSpan.FromSeconds(5) };

        var updated = original with { ProcessingDuration = TimeSpan.FromSeconds(20) };

        updated.ProcessingDuration.Should().Be(TimeSpan.FromSeconds(20));
        original.ProcessingDuration.Should().Be(TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void VideoProcessedEvent_TwoEqualInstances_ShouldHaveSameHashCode()
    {
        var videoId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var date = new DateTime(2024, 8, 1, 0, 0, 0, DateTimeKind.Utc);
        var duration = TimeSpan.FromSeconds(7);

        var e1 = new VideoProcessedEvent { VideoId = videoId, UserId = userId, Success = true, FrameCount = 90, ProcessedAt = date, ProcessingDuration = duration };
        var e2 = new VideoProcessedEvent { VideoId = videoId, UserId = userId, Success = true, FrameCount = 90, ProcessedAt = date, ProcessingDuration = duration };

        e1.GetHashCode().Should().Be(e2.GetHashCode());
    }

    [Fact]
    public void VideoProcessedEvent_ToString_ShouldContainTypeName()
    {
        var evt = new VideoProcessedEvent { Success = true, FrameCount = 60 };

        var result = evt.ToString();

        result.Should().Contain("VideoProcessedEvent");
        result.Should().Contain("True");
    }

    [Fact]
    public void VideoProcessedEvent_ToString_WhenFailed_ShouldContainFalse()
    {
        var evt = new VideoProcessedEvent { Success = false, ErrorMessage = "Unknown error" };

        var result = evt.ToString();

        result.Should().Contain("VideoProcessedEvent");
        result.Should().Contain("False");
    }
}
