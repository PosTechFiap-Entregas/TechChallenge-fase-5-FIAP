using FiapX.Application.DTOs;
using FiapX.Application.UseCases.Videos;
using FiapX.Domain.Entities;
using FiapX.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace FiapX.Application.Tests.UseCases.Videos;

public class DownloadVideoUseCaseTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IStorageService> _storageServiceMock;
    private readonly Mock<IVideoRepository> _videoRepositoryMock;
    private readonly DownloadVideoUseCase _sut;

    public DownloadVideoUseCaseTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _storageServiceMock = new Mock<IStorageService>();
        _videoRepositoryMock = new Mock<IVideoRepository>();

        _unitOfWorkMock.Setup(x => x.Videos).Returns(_videoRepositoryMock.Object);

        _sut = new DownloadVideoUseCase(_unitOfWorkMock.Object, _storageServiceMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidCompletedVideo_ShouldReturnDownloadResponse()
    {
        // Arrange
        var videoId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var video = new Video(userId, "video.mp4", "/storage/video.mp4", 1024);
        video.MarkAsQueued();
        video.StartProcessing();
        video.CompleteProcessing("/storage/frames.zip", 100, TimeSpan.FromSeconds(10));

        var fileStream = new MemoryStream();
        var fileSize = 1024L;

        _videoRepositoryMock
            .Setup(x => x.GetByIdWithUserAsync(videoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(video);

        _storageServiceMock
            .Setup(x => x.FileExistsAsync("/storage/frames.zip", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _storageServiceMock
            .Setup(x => x.GetFileAsync("/storage/frames.zip", It.IsAny<CancellationToken>()))
            .ReturnsAsync(fileStream);

        _storageServiceMock
            .Setup(x => x.GetFileSizeAsync("/storage/frames.zip", It.IsAny<CancellationToken>()))
            .ReturnsAsync(fileSize);

        // Act
        var result = await _sut.ExecuteAsync(videoId, userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.FileStream.Should().NotBeNull();
        result.Value.FileName.Should().Be("frames.zip");
        result.Value.FileSize.Should().Be(fileSize);
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
        var requesterId = Guid.NewGuid(); // Diferente do dono

        var video = new Video(ownerId, "video.mp4", "/storage/video.mp4", 1024);
        video.MarkAsQueued();
        video.StartProcessing();
        video.CompleteProcessing("/storage/frames.zip", 100, TimeSpan.FromSeconds(10));

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
    public async Task ExecuteAsync_WithUnprocessedVideo_ShouldReturnFailure()
    {
        // Arrange
        var videoId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var video = new Video(userId, "video.mp4", "/storage/video.mp4", 1024);
        // Status: Uploaded (não processado)

        _videoRepositoryMock
            .Setup(x => x.GetByIdWithUserAsync(videoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(video);

        // Act
        var result = await _sut.ExecuteAsync(videoId, userId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Vídeo ainda não foi processado ou falhou no processamento.");
    }

    [Fact]
    public async Task ExecuteAsync_WithFailedVideo_ShouldReturnFailure()
    {
        // Arrange
        var videoId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var video = new Video(userId, "video.mp4", "/storage/video.mp4", 1024);
        video.MarkAsQueued();
        video.StartProcessing();
        video.FailProcessing("Error");

        _videoRepositoryMock
            .Setup(x => x.GetByIdWithUserAsync(videoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(video);

        // Act
        var result = await _sut.ExecuteAsync(videoId, userId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Vídeo ainda não foi processado ou falhou no processamento.");
    }

    [Fact]
    public async Task ExecuteAsync_WhenZipFileNotExists_ShouldReturnFailure()
    {
        // Arrange
        var videoId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var video = new Video(userId, "video.mp4", "/storage/video.mp4", 1024);
        video.MarkAsQueued();
        video.StartProcessing();
        video.CompleteProcessing("/storage/frames.zip", 100, TimeSpan.FromSeconds(10));

        _videoRepositoryMock
            .Setup(x => x.GetByIdWithUserAsync(videoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(video);

        _storageServiceMock
            .Setup(x => x.FileExistsAsync("/storage/frames.zip", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _sut.ExecuteAsync(videoId, userId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Arquivo ZIP não encontrado no storage.");
    }
}