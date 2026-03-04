using FiapX.Application.Events;
using FiapX.Domain.Entities;
using FiapX.Domain.Enums;
using FiapX.Domain.Interfaces;
using FiapX.Worker.Consumers;
using FiapX.Worker.Services;
using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;

namespace FiapX.Worker.Tests.Consumers;

public class VideoUploadedEventConsumerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IStorageService> _storageServiceMock;
    private readonly Mock<IVideoProcessingService> _videoProcessingServiceMock;
    private readonly Mock<IMessagePublisher> _messagePublisherMock;
    private readonly Mock<ITelegramNotificationService> _telegramServiceMock;
    private readonly Mock<ILogger<VideoUploadedEventConsumer>> _loggerMock;
    private readonly VideoMetricsService _metrics;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IVideoRepository> _videoRepositoryMock;
    private readonly VideoUploadedEventConsumer _sut;

    public VideoUploadedEventConsumerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _storageServiceMock = new Mock<IStorageService>();
        _videoProcessingServiceMock = new Mock<IVideoProcessingService>();
        _messagePublisherMock = new Mock<IMessagePublisher>();
        _telegramServiceMock = new Mock<ITelegramNotificationService>();
        _loggerMock = new Mock<ILogger<VideoUploadedEventConsumer>>();
        _metrics = new VideoMetricsService();
        _userRepositoryMock = new Mock<IUserRepository>();
        _videoRepositoryMock = new Mock<IVideoRepository>();

        _unitOfWorkMock.Setup(x => x.Users).Returns(_userRepositoryMock.Object);
        _unitOfWorkMock.Setup(x => x.Videos).Returns(_videoRepositoryMock.Object);

        _sut = new VideoUploadedEventConsumer(
            _unitOfWorkMock.Object,
            _storageServiceMock.Object,
            _videoProcessingServiceMock.Object,
            _messagePublisherMock.Object,
            _telegramServiceMock.Object,
            _loggerMock.Object,
            _metrics);
    }

    [Fact]
    public async Task Consume_WithValidVideo_ShouldProcessSuccessfully()
    {
        var videoId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var user = new User("user@test.com", "hash", "Test User");
        var video = new Video(userId, "video.mp4", "/storage/video.mp4", 1024);
        video.MarkAsQueued();

        var message = new VideoUploadedEvent
        {
            VideoId = videoId,
            UserId = userId,
            StoragePath = "/storage/video.mp4",
            OriginalFileName = "video.mp4",
            FileSizeBytes = 1024
        };

        var context = CreateConsumeContext(message);

        var tempZipPath = Path.Combine(Path.GetTempPath(), "test_frames.zip");
        File.WriteAllText(tempZipPath, "fake zip content");

        var processingResult = new VideoProcessingResult
        {
            Success = true,
            ZipPath = tempZipPath,
            FrameCount = 100,
            ProcessingDuration = TimeSpan.FromSeconds(10)
        };

        _videoRepositoryMock
            .Setup(x => x.GetByIdAsync(videoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(video);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _storageServiceMock
            .Setup(x => x.GetTempDirectory())
            .Returns(Path.GetTempPath());

        _videoProcessingServiceMock
            .Setup(x => x.ProcessVideoAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(processingResult);

        _storageServiceMock
            .Setup(x => x.SaveZipAsync(
                It.IsAny<Stream>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync("/storage/frames.zip");

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        try
        {
            await _sut.Consume(context.Object);

            video.Status.Should().Be(VideoStatus.Completed);
            video.ZipPath.Should().Be("/storage/frames.zip");
            video.FrameCount.Should().Be(100);

            _telegramServiceMock.Verify(
                x => x.NotifyVideoProcessingSuccessAsync(
                    video.Id,
                    "video.mp4",
                    "Test User",
                    100,
                    It.IsAny<TimeSpan>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _messagePublisherMock.Verify(
                x => x.PublishAsync(
                    It.Is<VideoProcessedEvent>(e => e.Success && e.VideoId == video.Id),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
        finally
        {
            if (File.Exists(tempZipPath))
                File.Delete(tempZipPath);
        }
    }

    [Fact]
    public async Task Consume_WithNonExistentVideo_ShouldLogAndReturn()
    {
        var message = new VideoUploadedEvent
        {
            VideoId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            StoragePath = "/storage/video.mp4",
            OriginalFileName = "video.mp4",
            FileSizeBytes = 1024
        };

        var context = CreateConsumeContext(message);

        _videoRepositoryMock
            .Setup(x => x.GetByIdAsync(message.VideoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Video?)null);

        await _sut.Consume(context.Object);

        _videoProcessingServiceMock.Verify(
            x => x.ProcessVideoAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Consume_WithAlreadyCompletedVideo_ShouldSkipProcessing()
    {
        var videoId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var video = new Video(userId, "video.mp4", "/storage/video.mp4", 1024);
        video.MarkAsQueued();
        video.StartProcessing();
        video.CompleteProcessing("/storage/frames.zip", 100, TimeSpan.FromSeconds(10));

        var message = new VideoUploadedEvent
        {
            VideoId = videoId,
            UserId = userId,
            StoragePath = "/storage/video.mp4",
            OriginalFileName = "video.mp4",
            FileSizeBytes = 1024
        };

        var context = CreateConsumeContext(message);

        _videoRepositoryMock
            .Setup(x => x.GetByIdAsync(videoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(video);

        await _sut.Consume(context.Object);

        _videoProcessingServiceMock.Verify(
            x => x.ProcessVideoAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Consume_WhenProcessingFails_ShouldMarkAsFailedAndNotify()
    {
        var videoId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var user = new User("user@test.com", "hash", "Test User");
        var video = new Video(userId, "video.mp4", "/storage/video.mp4", 1024);
        video.MarkAsQueued();

        var message = new VideoUploadedEvent
        {
            VideoId = videoId,
            UserId = userId,
            StoragePath = "/storage/video.mp4",
            OriginalFileName = "video.mp4",
            FileSizeBytes = 1024
        };

        var context = CreateConsumeContext(message);

        var processingResult = new VideoProcessingResult
        {
            Success = false,
            ErrorMessage = "FFmpeg error",
            ProcessingDuration = TimeSpan.FromSeconds(5)
        };

        _videoRepositoryMock
            .Setup(x => x.GetByIdAsync(videoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(video);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _storageServiceMock
            .Setup(x => x.GetTempDirectory())
            .Returns(Path.GetTempPath());

        _videoProcessingServiceMock
            .Setup(x => x.ProcessVideoAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(processingResult);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        await _sut.Consume(context.Object);

        video.Status.Should().Be(VideoStatus.Failed);
        video.ErrorMessage.Should().Be("FFmpeg error");

        _telegramServiceMock.Verify(
            x => x.NotifyVideoProcessingErrorAsync(
                video.Id,
                "video.mp4",
                "Test User",
                "FFmpeg error",
                It.IsAny<CancellationToken>()),
            Times.Once);

        _messagePublisherMock.Verify(
            x => x.PublishAsync(
                It.Is<VideoProcessedEvent>(e => !e.Success && e.ErrorMessage == "FFmpeg error"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Consume_WhenExceptionOccurs_ShouldRethrow()
    {
        var videoId = Guid.NewGuid();

        var message = new VideoUploadedEvent
        {
            VideoId = videoId,
            UserId = Guid.NewGuid(),
            StoragePath = "/storage/video.mp4",
            OriginalFileName = "video.mp4",
            FileSizeBytes = 1024
        };

        var context = CreateConsumeContext(message);

        _videoRepositoryMock
            .Setup(x => x.GetByIdAsync(videoId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        await Assert.ThrowsAsync<Exception>(() => _sut.Consume(context.Object));
    }

    private Mock<ConsumeContext<VideoUploadedEvent>> CreateConsumeContext(VideoUploadedEvent message)
    {
        var contextMock = new Mock<ConsumeContext<VideoUploadedEvent>>();
        contextMock.Setup(x => x.Message).Returns(message);
        contextMock.Setup(x => x.MessageId).Returns(Guid.NewGuid());
        contextMock.Setup(x => x.CancellationToken).Returns(CancellationToken.None);
        return contextMock;
    }
}