using FiapX.Application.DTOs;
using FluentAssertions;

namespace FiapX.Application.Tests.DTOs;

public class UploadVideoRequestTests
{
    [Fact]
    public void UploadVideoRequest_DefaultValues_ShouldBeInitializedCorrectly()
    {
        var request = new UploadVideoRequest();

        request.FileStream.Should().BeSameAs(Stream.Null);
        request.FileName.Should().Be(string.Empty);
        request.FileSize.Should().Be(0L);
        request.UserId.Should().Be(Guid.Empty);
    }

    [Fact]
    public void UploadVideoRequest_WithAllPropertiesSet_ShouldReturnCorrectValues()
    {
        var userId = Guid.NewGuid();
        using var stream = new MemoryStream(new byte[] { 1, 2, 3 });

        var request = new UploadVideoRequest
        {
            FileStream = stream,
            FileName = "video.mp4",
            FileSize = 1024L,
            UserId = userId
        };

        request.FileStream.Should().BeSameAs(stream);
        request.FileName.Should().Be("video.mp4");
        request.FileSize.Should().Be(1024L);
        request.UserId.Should().Be(userId);
    }

    [Fact]
    public void UploadVideoRequest_FileStream_DefaultShouldBeStreamNull()
    {
        var request = new UploadVideoRequest();

        request.FileStream.Should().NotBeNull();
        request.FileStream.Should().BeSameAs(Stream.Null);
    }

    [Fact]
    public void UploadVideoRequest_FileStream_ShouldAcceptMemoryStream()
    {
        using var ms = new MemoryStream();

        var request = new UploadVideoRequest { FileStream = ms };

        request.FileStream.Should().BeOfType<MemoryStream>();
    }

    [Theory]
    [InlineData(0L)]
    [InlineData(1L)]
    [InlineData(104857600L)]
    [InlineData(long.MaxValue)]
    public void UploadVideoRequest_FileSize_ShouldAcceptValidValues(long size)
    {
        var request = new UploadVideoRequest { FileSize = size };

        request.FileSize.Should().Be(size);
    }

    [Theory]
    [InlineData("video.mp4")]
    [InlineData("my video with spaces.avi")]
    [InlineData("arquivo_com_underscore.mov")]
    [InlineData("file.with.multiple.dots.mkv")]
    public void UploadVideoRequest_FileName_ShouldAcceptVariousFormats(string fileName)
    {
        var request = new UploadVideoRequest { FileName = fileName };

        request.FileName.Should().Be(fileName);
    }

    [Fact]
    public void UploadVideoRequest_TwoInstancesWithSameValues_ShouldBeEqual()
    {
        var userId = Guid.NewGuid();
        var stream = Stream.Null;

        var r1 = new UploadVideoRequest { FileStream = stream, FileName = "v.mp4", FileSize = 100, UserId = userId };
        var r2 = new UploadVideoRequest { FileStream = stream, FileName = "v.mp4", FileSize = 100, UserId = userId };

        r1.Should().Be(r2);
    }

    [Fact]
    public void UploadVideoRequest_TwoInstancesWithDifferentFileName_ShouldNotBeEqual()
    {
        var r1 = new UploadVideoRequest { FileName = "a.mp4" };
        var r2 = new UploadVideoRequest { FileName = "b.mp4" };

        r1.Should().NotBe(r2);
    }

    [Fact]
    public void UploadVideoRequest_TwoInstancesWithDifferentFileSize_ShouldNotBeEqual()
    {
        var r1 = new UploadVideoRequest { FileSize = 100 };
        var r2 = new UploadVideoRequest { FileSize = 200 };

        r1.Should().NotBe(r2);
    }

    [Fact]
    public void UploadVideoRequest_TwoInstancesWithDifferentUserId_ShouldNotBeEqual()
    {
        var r1 = new UploadVideoRequest { UserId = Guid.NewGuid() };
        var r2 = new UploadVideoRequest { UserId = Guid.NewGuid() };

        r1.Should().NotBe(r2);
    }

    [Fact]
    public void UploadVideoRequest_WithExpression_ShouldCreateNewInstanceWithUpdatedFileName()
    {
        var original = new UploadVideoRequest { FileName = "old.mp4", FileSize = 500 };

        var updated = original with { FileName = "new.mp4" };

        updated.FileName.Should().Be("new.mp4");
        updated.FileSize.Should().Be(500);
        original.FileName.Should().Be("old.mp4");
    }

    [Fact]
    public void UploadVideoRequest_ToString_ShouldContainTypeName()
    {
        var request = new UploadVideoRequest { FileName = "clip.mp4" };

        var result = request.ToString();

        result.Should().Contain("UploadVideoRequest");
        result.Should().Contain("clip.mp4");
    }
}
