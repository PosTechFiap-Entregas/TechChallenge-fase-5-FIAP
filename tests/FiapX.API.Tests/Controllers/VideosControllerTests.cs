using FiapX.API.Controllers;
using FiapX.Application.DTOs;
using FiapX.Application.Interfaces.UseCases;
using FiapX.Shared.Results;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace FiapX.API.Tests.Controllers;

public class VideosControllerTests
{
    private readonly Mock<IUploadVideoUseCase> _uploadUseCaseMock;
    private readonly Mock<IGetUserVideosUseCase> _getUserVideosUseCaseMock;
    private readonly Mock<IGetVideoStatusUseCase> _getVideoStatusUseCaseMock;
    private readonly Mock<IDownloadVideoUseCase> _downloadUseCaseMock;
    private readonly Mock<IValidator<UploadVideoRequest>> _uploadValidatorMock;
    private readonly VideosController _sut;
    private readonly Guid _userId = Guid.NewGuid();

    public VideosControllerTests()
    {
        _uploadUseCaseMock = new Mock<IUploadVideoUseCase>();
        _getUserVideosUseCaseMock = new Mock<IGetUserVideosUseCase>();
        _getVideoStatusUseCaseMock = new Mock<IGetVideoStatusUseCase>();
        _downloadUseCaseMock = new Mock<IDownloadVideoUseCase>();
        _uploadValidatorMock = new Mock<IValidator<UploadVideoRequest>>();

        _sut = new VideosController(
            _uploadUseCaseMock.Object,
            _getUserVideosUseCaseMock.Object,
            _getVideoStatusUseCaseMock.Object,
            _downloadUseCaseMock.Object,
            _uploadValidatorMock.Object);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, _userId.ToString())
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };
    }

    [Fact]
    public async Task Upload_WithValidFile_ShouldReturnCreated()
    {
        var fileMock = new Mock<IFormFile>();
        var content = "fake video content"u8.ToArray();
        var ms = new MemoryStream(content);

        fileMock.Setup(f => f.FileName).Returns("video.mp4");
        fileMock.Setup(f => f.Length).Returns(content.Length);
        fileMock.Setup(f => f.OpenReadStream()).Returns(ms);

        var response = new UploadVideoResponse
        {
            VideoId = Guid.NewGuid(),
            OriginalFileName = "video.mp4",
            Status = "Queued",
            Message = "Vídeo enviado com sucesso"
        };

        _uploadValidatorMock
            .Setup(x => x.ValidateAsync(It.IsAny<UploadVideoRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _uploadUseCaseMock
            .Setup(x => x.ExecuteAsync(It.IsAny<UploadVideoRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(response));

        var result = await _sut.Upload(fileMock.Object, CancellationToken.None);

        result.Should().BeOfType<CreatedResult>();
    }

    [Fact]
    public async Task ListVideos_ShouldReturnOkWithVideos()
    {
        var videos = new List<VideoListResponse>
        {
            new()
            {
                VideoId = Guid.NewGuid(),
                OriginalFileName = "video1.mp4",
                Status = "Completed",
                FileSizeMB = 10
            }
        };

        _getUserVideosUseCaseMock
            .Setup(x => x.ExecuteAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<IEnumerable<VideoListResponse>>(videos));

        var result = await _sut.ListVideos(CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetStatus_WithValidVideoId_ShouldReturnOk()
    {
        var videoId = Guid.NewGuid();
        var statusResponse = new VideoStatusResponse
        {
            VideoId = videoId,
            OriginalFileName = "video.mp4",
            Status = "Completed",
            CanDownload = true
        };

        _getVideoStatusUseCaseMock
            .Setup(x => x.ExecuteAsync(videoId, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(statusResponse));

        var result = await _sut.GetStatus(videoId, CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Download_WithValidVideoId_ShouldReturnFile()
    {
        var videoId = Guid.NewGuid();
        var fileStream = new MemoryStream("zip content"u8.ToArray());

        var downloadResponse = new VideoDownloadResponse
        {
            FileStream = fileStream,
            FileName = "frames.zip",
            FileSize = fileStream.Length
        };

        _downloadUseCaseMock
            .Setup(x => x.ExecuteAsync(videoId, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(downloadResponse));

        var result = await _sut.Download(videoId, CancellationToken.None);

        result.Should().BeOfType<FileStreamResult>();
        var fileResult = result as FileStreamResult;
        fileResult!.FileDownloadName.Should().Be("frames.zip");
        fileResult.ContentType.Should().Be("application/zip");
    }
}