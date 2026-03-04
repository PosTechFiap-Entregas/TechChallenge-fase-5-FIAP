using FiapX.Application.DTOs;
using FiapX.Application.Events;
using FiapX.Application.UseCases.Videos;
using FiapX.Domain.Entities;
using FiapX.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace FiapX.Application.Tests.UseCases.Videos;

public class UploadVideoUseCaseTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IStorageService> _storageServiceMock;
    private readonly Mock<IMessagePublisher> _messagePublisherMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IVideoRepository> _videoRepositoryMock;
    private readonly UploadVideoUseCase _sut;

    public UploadVideoUseCaseTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _storageServiceMock = new Mock<IStorageService>();
        _messagePublisherMock = new Mock<IMessagePublisher>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _videoRepositoryMock = new Mock<IVideoRepository>();

        _unitOfWorkMock.Setup(x => x.Users).Returns(_userRepositoryMock.Object);
        _unitOfWorkMock.Setup(x => x.Videos).Returns(_videoRepositoryMock.Object);

        _sut = new UploadVideoUseCase(
            _unitOfWorkMock.Object,
            _storageServiceMock.Object,
            _messagePublisherMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidData_ShouldUploadVideoAndPublishEvent()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User("user@test.com", "hash", "User");
        var fileName = "video.mp4";
        var fileSize = 1024 * 1024L; // 1MB
        var storagePath = "/storage/video.mp4";

        var fileStream = new MemoryStream();
        var request = new UploadVideoRequest
        {
            UserId = userId,
            FileName = fileName,
            FileSize = fileSize,
            FileStream = fileStream
        };

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _storageServiceMock
            .Setup(x => x.SaveVideoAsync(fileStream, fileName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(storagePath);

        _videoRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Video>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _messagePublisherMock
            .Setup(x => x.PublishAsync(It.IsAny<VideoUploadedEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.ExecuteAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.VideoId.Should().NotBe(Guid.Empty);
        result.Value.OriginalFileName.Should().Be(fileName);
        result.Value.Status.Should().Be("Queued");
        result.Value.Message.Should().Contain("processado");

        _storageServiceMock.Verify(
            x => x.SaveVideoAsync(fileStream, fileName, It.IsAny<CancellationToken>()),
            Times.Once);

        _videoRepositoryMock.Verify(
            x => x.AddAsync(It.Is<Video>(v =>
                v.UserId == userId &&
                v.OriginalFileName == fileName &&
                v.StoragePath == storagePath &&
                v.FileSizeBytes == fileSize), It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);

        _messagePublisherMock.Verify(
            x => x.PublishAsync(It.Is<VideoUploadedEvent>(e =>
                e.UserId == userId &&
                e.OriginalFileName == fileName &&
                e.StoragePath == storagePath &&
                e.FileSizeBytes == fileSize), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithNonExistentUser_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new UploadVideoRequest
        {
            UserId = userId,
            FileName = "video.mp4",
            FileSize = 1024,
            FileStream = new MemoryStream()
        };

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _sut.ExecuteAsync(request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Usuário não encontrado.");

        _storageServiceMock.Verify(
            x => x.SaveVideoAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _messagePublisherMock.Verify(
            x => x.PublishAsync(It.IsAny<VideoUploadedEvent>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenStorageFails_ShouldThrowException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User("user@test.com", "hash", "User");
        var request = new UploadVideoRequest
        {
            UserId = userId,
            FileName = "video.mp4",
            FileSize = 1024,
            FileStream = new MemoryStream()
        };

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _storageServiceMock
            .Setup(x => x.SaveVideoAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new IOException("Disk full"));

        // Act & Assert
        await Assert.ThrowsAsync<IOException>(
            async () => await _sut.ExecuteAsync(request));

        _videoRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Video>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData("video.mp4", 1024L)]
    [InlineData("movie.avi", 1024L * 1024L * 100)] // 100MB
    [InlineData("clip.mov", 500L)]
    public async Task ExecuteAsync_WithVariousFileSizes_ShouldSucceed(string fileName, long fileSize)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User("user@test.com", "hash", "User");
        var request = new UploadVideoRequest
        {
            UserId = userId,
            FileName = fileName,
            FileSize = fileSize,
            FileStream = new MemoryStream()
        };

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _storageServiceMock
            .Setup(x => x.SaveVideoAsync(It.IsAny<Stream>(), fileName, It.IsAny<CancellationToken>()))
            .ReturnsAsync($"/storage/{fileName}");

        _videoRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Video>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _messagePublisherMock
            .Setup(x => x.PublishAsync(It.IsAny<VideoUploadedEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.ExecuteAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.OriginalFileName.Should().Be(fileName);
    }
}