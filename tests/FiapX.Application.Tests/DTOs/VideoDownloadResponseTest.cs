using FiapX.Application.DTOs;
using FluentAssertions;

namespace FiapX.Application.Tests.DTOs;

public class VideoDownloadResponseTests
{
    [Fact]
    public void VideoDownloadResponse_DefaultValues_ShouldBeInitializedCorrectly()
    {
        var response = new VideoDownloadResponse();

        response.FileStream.Should().BeSameAs(Stream.Null);
        response.FileName.Should().Be(string.Empty);
        response.FileSize.Should().Be(0L);
    }

    [Fact]
    public void VideoDownloadResponse_WithAllPropertiesSet_ShouldReturnCorrectValues()
    {
        using var stream = new MemoryStream(new byte[] { 0x50, 0x4B });

        var response = new VideoDownloadResponse
        {
            FileStream = stream,
            FileName = "frames.zip",
            FileSize = 2048L
        };

        response.FileStream.Should().BeSameAs(stream);
        response.FileName.Should().Be("frames.zip");
        response.FileSize.Should().Be(2048L);
    }

    [Fact]
    public void VideoDownloadResponse_FileStream_DefaultShouldBeStreamNull()
    {
        var response = new VideoDownloadResponse();

        response.FileStream.Should().NotBeNull();
        response.FileStream.Should().BeSameAs(Stream.Null);
    }

    [Fact]
    public void VideoDownloadResponse_FileStream_ShouldAcceptMemoryStream()
    {
        using var ms = new MemoryStream();
        ms.Write(new byte[] { 1, 2, 3, 4 });

        var response = new VideoDownloadResponse { FileStream = ms };

        response.FileStream.Should().BeOfType<MemoryStream>();
        response.FileStream.Length.Should().Be(4);
    }

    [Theory]
    [InlineData(0L)]
    [InlineData(1L)]
    [InlineData(52428800L)]
    [InlineData(1073741824L)]
    [InlineData(long.MaxValue)]
    public void VideoDownloadResponse_FileSize_ShouldAcceptValidValues(long size)
    {
        var response = new VideoDownloadResponse { FileSize = size };

        response.FileSize.Should().Be(size);
    }

    [Theory]
    [InlineData("frames.zip")]
    [InlineData("video_frames_2024.zip")]
    [InlineData("output with spaces.zip")]
    [InlineData("arquivo.ZIP")]
    public void VideoDownloadResponse_FileName_ShouldAcceptVariousFormats(string fileName)
    {
        var response = new VideoDownloadResponse { FileName = fileName };

        response.FileName.Should().Be(fileName);
    }

    [Fact]
    public void VideoDownloadResponse_TwoInstancesWithSameStreamReference_ShouldBeEqual()
    {
        var sharedStream = Stream.Null;

        var r1 = new VideoDownloadResponse { FileStream = sharedStream, FileName = "f.zip", FileSize = 100 };
        var r2 = new VideoDownloadResponse { FileStream = sharedStream, FileName = "f.zip", FileSize = 100 };

        r1.Should().Be(r2);
    }

    [Fact]
    public void VideoDownloadResponse_TwoInstancesWithDifferentStreamReferences_ShouldNotBeEqual()
    {
        using var stream1 = new MemoryStream();
        using var stream2 = new MemoryStream();

        var r1 = new VideoDownloadResponse { FileStream = stream1, FileName = "f.zip", FileSize = 100 };
        var r2 = new VideoDownloadResponse { FileStream = stream2, FileName = "f.zip", FileSize = 100 };

        r1.Should().NotBe(r2);
    }

    [Fact]
    public void VideoDownloadResponse_TwoInstancesWithDifferentFileName_ShouldNotBeEqual()
    {
        var r1 = new VideoDownloadResponse { FileName = "a.zip" };
        var r2 = new VideoDownloadResponse { FileName = "b.zip" };

        r1.Should().NotBe(r2);
    }

    [Fact]
    public void VideoDownloadResponse_TwoInstancesWithDifferentFileSize_ShouldNotBeEqual()
    {
        var r1 = new VideoDownloadResponse { FileName = "f.zip", FileSize = 100 };
        var r2 = new VideoDownloadResponse { FileName = "f.zip", FileSize = 200 };

        r1.Should().NotBe(r2);
    }

    [Fact]
    public void VideoDownloadResponse_WithExpression_ShouldUpdateFileSizeAndPreserveOtherFields()
    {
        var original = new VideoDownloadResponse { FileName = "frames.zip", FileSize = 1000 };

        var updated = original with { FileSize = 2000 };

        updated.FileSize.Should().Be(2000);
        updated.FileName.Should().Be("frames.zip");
        original.FileSize.Should().Be(1000);
    }

    [Fact]
    public void VideoDownloadResponse_ToString_ShouldContainTypeName()
    {
        var response = new VideoDownloadResponse { FileName = "frames.zip", FileSize = 512 };

        var result = response.ToString();

        result.Should().Contain("VideoDownloadResponse");
        result.Should().Contain("frames.zip");
    }
}
