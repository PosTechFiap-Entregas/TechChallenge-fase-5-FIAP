using FiapX.Application.DTOs;
using FiapX.Application.UseCases.Videos;
using FiapX.Domain.Entities;
using FiapX.Domain.Enums;
using FiapX.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace FiapX.Application.Tests.UseCases.Videos;

public class GetVideoStatusUseCaseTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IVideoRepository> _videoRepositoryMock;
    private readonly GetVideoStatusUseCase _sut;

    public GetVideoStatusUseCaseTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _videoRepositoryMock = new Mock<IVideoRepository>();

        _unitOfWorkMock.Setup(x => x.Videos).Returns(_videoRepositoryMock.Object);

        _sut = new GetVideoStatusUseCase(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidVideo_ShouldReturnStatus()
    {
        // Arrange
        var videoId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var video = new Video(userId, "video.mp4", "/storage/video.mp4", 1024);
        video.MarkAsQueued();
        video.StartProcessing();
        video.CompleteProcessing("/storage/frames.zip", 100, TimeSpan.FromSeconds(15));

        _videoRepositoryMock
            .Setup(x => x.GetByIdWithUserAsync(videoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(video);

        // Act
        var result = await _sut.ExecuteAsync(videoId, userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.VideoId.Should().Be(video.Id);
        result.Value.OriginalFileName.Should().Be("video.mp4");
        result.Value.Status.Should().Be("Completed");
        result.Value.StatusDescription.Should().Be("Processamento concluído");
        result.Value.FrameCount.Should().Be(100);
        result.Value.ProcessingDurationSeconds.Should().Be(15);
        result.Value.CanDownload.Should().BeTrue();
        result.Value.ProcessedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task ExecuteAsync_WithNonExistentVideo_ShouldReturnFailure()
    {
        // Arrange
        var videoId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _videoRepositoryMock
            .Setup(x => x.GetByIdWithUserAsync(videoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Video?)null);

        // Act
        var result = await _sut.ExecuteAsync(videoId, userId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Vídeo não encontrado.");
    }

    [Fact]
    public async Task ExecuteAsync_WithDifferentUserId_ShouldReturnAccessDenied()
    {
        // Arrange
        var videoId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var requesterId = Guid.NewGuid();

        var video = new Video(ownerId, "video.mp4", "/storage/video.mp4", 1024);

        _videoRepositoryMock
            .Setup(x => x.GetByIdWithUserAsync(videoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(video);

        // Act
        var result = await _sut.ExecuteAsync(videoId, requesterId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Acesso negado.");
    }

    [Fact]
    public async Task ExecuteAsync_WithFailedVideo_ShouldReturnFailureStatus()
    {
        // Arrange
        var videoId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var video = new Video(userId, "video.mp4", "/storage/video.mp4", 1024);
        video.MarkAsQueued();
        video.StartProcessing();
        video.FailProcessing("FFmpeg error");

        _videoRepositoryMock
            .Setup(x => x.GetByIdWithUserAsync(videoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(video);

        // Act
        var result = await _sut.ExecuteAsync(videoId, userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Failed");
        result.Value.StatusDescription.Should().Be("Falha no processamento");
        result.Value.ErrorMessage.Should().Be("FFmpeg error");
        result.Value.CanDownload.Should().BeFalse();
    }

    [Theory]
    [InlineData(VideoStatus.Uploaded, "Enviado")]
    [InlineData(VideoStatus.Queued, "Na fila de processamento")]
    [InlineData(VideoStatus.Processing, "Processando frames...")]
    public async Task ExecuteAsync_ShouldMapStatusDescriptionCorrectly(VideoStatus status, string expectedDescription)
    {
        // Arrange
        var videoId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var video = new Video(userId, "video.mp4", "/storage/video.mp4", 1024);

        if (status == VideoStatus.Queued)
            video.MarkAsQueued();
        else if (status == VideoStatus.Processing)
        {
            video.MarkAsQueued();
            video.StartProcessing();
        }

        _videoRepositoryMock
            .Setup(x => x.GetByIdWithUserAsync(videoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(video);

        // Act
        var result = await _sut.ExecuteAsync(videoId, userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.StatusDescription.Should().Be(expectedDescription);
    }
}