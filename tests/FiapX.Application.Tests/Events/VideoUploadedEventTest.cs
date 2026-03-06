using FiapX.Application.Events;
using FluentAssertions;

namespace FiapX.Application.Tests.Events;

public class VideoUploadedEventTests
{
    [Fact]
    public void VideoUploadedEvent_DefaultValues_ShouldBeInitializedCorrectly()
    {
        var evt = new VideoUploadedEvent();

        evt.VideoId.Should().Be(Guid.Empty);
        evt.UserId.Should().Be(Guid.Empty);
        evt.StoragePath.Should().Be(string.Empty);
        evt.OriginalFileName.Should().Be(string.Empty);
        evt.FileSizeBytes.Should().Be(0L);
        evt.UploadedAt.Should().Be(default(DateTime));
    }

    [Fact]
    public void VideoUploadedEvent_WithAllPropertiesSet_ShouldReturnCorrectValues()
    {
        var videoId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var uploadedAt = DateTime.UtcNow;

        var evt = new VideoUploadedEvent
        {
            VideoId = videoId,
            UserId = userId,
            StoragePath = "/storage/videos/video.mp4",
            OriginalFileName = "video.mp4",
            FileSizeBytes = 52428800L,
            UploadedAt = uploadedAt
        };

        evt.VideoId.Should().Be(videoId);
        evt.UserId.Should().Be(userId);
        evt.StoragePath.Should().Be("/storage/videos/video.mp4");
        evt.OriginalFileName.Should().Be("video.mp4");
        evt.FileSizeBytes.Should().Be(52428800L);
        evt.UploadedAt.Should().Be(uploadedAt);
    }

    [Fact]
    public void VideoUploadedEvent_VideoId_ShouldAcceptValidGuid()
    {
        var guid = Guid.NewGuid();

        var evt = new VideoUploadedEvent { VideoId = guid };

        evt.VideoId.Should().NotBe(Guid.Empty);
        evt.VideoId.Should().Be(guid);
    }

    [Fact]
    public void VideoUploadedEvent_UserId_ShouldAcceptValidGuid()
    {
        var guid = Guid.NewGuid();

        var evt = new VideoUploadedEvent { UserId = guid };

        evt.UserId.Should().NotBe(Guid.Empty);
        evt.UserId.Should().Be(guid);
    }

    [Fact]
    public void VideoUploadedEvent_VideoId_And_UserId_ShouldBeIndependent()
    {
        var videoId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var evt = new VideoUploadedEvent { VideoId = videoId, UserId = userId };

        evt.VideoId.Should().NotBe(evt.UserId);
    }

    [Theory]
    [InlineData("/storage/videos/abc.mp4")]
    [InlineData("C:\\Videos\\upload.mp4")]
    [InlineData("uploads/2024/01/video.mp4")]
    [InlineData("")]
    public void VideoUploadedEvent_StoragePath_ShouldAcceptVariousFormats(string path)
    {
        var evt = new VideoUploadedEvent { StoragePath = path };

        evt.StoragePath.Should().Be(path);
    }

    [Theory]
    [InlineData("video.mp4")]
    [InlineData("my recording.avi")]
    [InlineData("file_with_underscore.mov")]
    [InlineData("arquivo.com.varios.pontos.mkv")]
    public void VideoUploadedEvent_OriginalFileName_ShouldAcceptVariousFormats(string fileName)
    {
        var evt = new VideoUploadedEvent { OriginalFileName = fileName };

        evt.OriginalFileName.Should().Be(fileName);
    }

    [Theory]
    [InlineData(0L)]
    [InlineData(1L)]
    [InlineData(1048576L)]
    [InlineData(104857600L)]
    [InlineData(1073741824L)]
    [InlineData(long.MaxValue)]
    public void VideoUploadedEvent_FileSizeBytes_ShouldAcceptValidValues(long size)
    {
        var evt = new VideoUploadedEvent { FileSizeBytes = size };

        evt.FileSizeBytes.Should().Be(size);
    }

    [Fact]
    public void VideoUploadedEvent_UploadedAt_ShouldAcceptUtcNow()
    {
        var now = DateTime.UtcNow;

        var evt = new VideoUploadedEvent { UploadedAt = now };

        evt.UploadedAt.Should().BeCloseTo(now, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void VideoUploadedEvent_UploadedAt_ShouldAcceptPastDate()
    {
        var past = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var evt = new VideoUploadedEvent { UploadedAt = past };

        evt.UploadedAt.Should().BeBefore(DateTime.UtcNow);
    }

    [Fact]
    public void VideoUploadedEvent_TwoInstancesWithSameValues_ShouldBeEqual()
    {
        var videoId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var uploadedAt = new DateTime(2024, 6, 1, 12, 0, 0, DateTimeKind.Utc);

        var e1 = new VideoUploadedEvent
        {
            VideoId = videoId,
            UserId = userId,
            StoragePath = "/path/video.mp4",
            OriginalFileName = "video.mp4",
            FileSizeBytes = 1024,
            UploadedAt = uploadedAt
        };

        var e2 = new VideoUploadedEvent
        {
            VideoId = videoId,
            UserId = userId,
            StoragePath = "/path/video.mp4",
            OriginalFileName = "video.mp4",
            FileSizeBytes = 1024,
            UploadedAt = uploadedAt
        };

        e1.Should().Be(e2);
        (e1 == e2).Should().BeTrue();
    }

    [Fact]
    public void VideoUploadedEvent_TwoInstancesWithDifferentVideoId_ShouldNotBeEqual()
    {
        var e1 = new VideoUploadedEvent { VideoId = Guid.NewGuid() };
        var e2 = new VideoUploadedEvent { VideoId = Guid.NewGuid() };

        e1.Should().NotBe(e2);
        (e1 != e2).Should().BeTrue();
    }

    [Fact]
    public void VideoUploadedEvent_TwoInstancesWithDifferentUserId_ShouldNotBeEqual()
    {
        var videoId = Guid.NewGuid();
        var e1 = new VideoUploadedEvent { VideoId = videoId, UserId = Guid.NewGuid() };
        var e2 = new VideoUploadedEvent { VideoId = videoId, UserId = Guid.NewGuid() };

        e1.Should().NotBe(e2);
    }

    [Fact]
    public void VideoUploadedEvent_TwoInstancesWithDifferentStoragePath_ShouldNotBeEqual()
    {
        var id = Guid.NewGuid();
        var e1 = new VideoUploadedEvent { VideoId = id, StoragePath = "/path/a.mp4" };
        var e2 = new VideoUploadedEvent { VideoId = id, StoragePath = "/path/b.mp4" };

        e1.Should().NotBe(e2);
    }

    [Fact]
    public void VideoUploadedEvent_TwoInstancesWithDifferentFileSizeBytes_ShouldNotBeEqual()
    {
        var id = Guid.NewGuid();
        var e1 = new VideoUploadedEvent { VideoId = id, FileSizeBytes = 100 };
        var e2 = new VideoUploadedEvent { VideoId = id, FileSizeBytes = 200 };

        e1.Should().NotBe(e2);
    }

    [Fact]
    public void VideoUploadedEvent_TwoInstancesWithDifferentUploadedAt_ShouldNotBeEqual()
    {
        var id = Guid.NewGuid();
        var e1 = new VideoUploadedEvent { VideoId = id, UploadedAt = DateTime.UtcNow };
        var e2 = new VideoUploadedEvent { VideoId = id, UploadedAt = DateTime.UtcNow.AddMinutes(-5) };

        e1.Should().NotBe(e2);
    }

    [Fact]
    public void VideoUploadedEvent_WithExpression_ShouldCreateNewInstanceWithUpdatedStoragePath()
    {
        var original = new VideoUploadedEvent
        {
            VideoId = Guid.NewGuid(),
            StoragePath = "/old/path.mp4",
            OriginalFileName = "video.mp4",
            FileSizeBytes = 512
        };

        var updated = original with { StoragePath = "/new/path.mp4" };

        updated.StoragePath.Should().Be("/new/path.mp4");
        updated.OriginalFileName.Should().Be("video.mp4");
        updated.FileSizeBytes.Should().Be(512);
        original.StoragePath.Should().Be("/old/path.mp4");
    }

    [Fact]
    public void VideoUploadedEvent_WithExpression_ShouldNotMutateOriginal()
    {
        var original = new VideoUploadedEvent { FileSizeBytes = 100, OriginalFileName = "v.mp4" };

        _ = original with { FileSizeBytes = 999 };

        original.FileSizeBytes.Should().Be(100);
    }

    [Fact]
    public void VideoUploadedEvent_TwoEqualInstances_ShouldHaveSameHashCode()
    {
        var videoId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var date = new DateTime(2024, 5, 20, 0, 0, 0, DateTimeKind.Utc);

        var e1 = new VideoUploadedEvent { VideoId = videoId, UserId = userId, FileSizeBytes = 256, UploadedAt = date };
        var e2 = new VideoUploadedEvent { VideoId = videoId, UserId = userId, FileSizeBytes = 256, UploadedAt = date };

        e1.GetHashCode().Should().Be(e2.GetHashCode());
    }

    [Fact]
    public void VideoUploadedEvent_ToString_ShouldContainTypeName()
    {
        var evt = new VideoUploadedEvent { OriginalFileName = "clip.mp4" };

        var result = evt.ToString();

        result.Should().Contain("VideoUploadedEvent");
        result.Should().Contain("clip.mp4");
    }
}
