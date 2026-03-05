using FiapX.Application.Events;
using FiapX.Infrastructure.Messaging;
using FluentAssertions;
using MassTransit;
using Moq;

namespace FiapX.Infrastructure.Tests.Messaging;

public class MassTransitMessagePublisherTests
{
    private readonly Mock<IBus> _busMock;
    private readonly MassTransitMessagePublisher _publisher;

    public MassTransitMessagePublisherTests()
    {
        _busMock = new Mock<IBus>();
        _publisher = new MassTransitMessagePublisher(_busMock.Object);
    }

    [Fact]
    public async Task PublishAsync_ShouldCallBusPublish()
    {
        // Arrange
        var message = new VideoUploadedEvent
        {
            VideoId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            StoragePath = "/path",
            OriginalFileName = "video.mp4",
            FileSizeBytes = 1024
        };

        _busMock
            .Setup(x => x.Publish(message, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _publisher.PublishAsync(message);

        // Assert
        _busMock.Verify(
            x => x.Publish(message, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task PublishAsync_WithCancellationToken_ShouldPassTokenToBus()
    {
        // Arrange
        var message = new VideoUploadedEvent
        {
            VideoId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            StoragePath = "/path",
            OriginalFileName = "video.mp4",
            FileSizeBytes = 1024
        };

        var cancellationToken = new CancellationToken();

        _busMock
            .Setup(x => x.Publish(message, cancellationToken))
            .Returns(Task.CompletedTask);

        // Act
        await _publisher.PublishAsync(message, cancellationToken);

        // Assert
        _busMock.Verify(
            x => x.Publish(message, cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task PublishAsync_WithDifferentMessageType_ShouldWork()
    {
        // Arrange
        var message = new VideoProcessedEvent
        {
            VideoId = Guid.NewGuid(),
            Success = true,
            ErrorMessage = null
        };

        _busMock
            .Setup(x => x.Publish(message, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _publisher.PublishAsync(message);

        // Assert
        _busMock.Verify(
            x => x.Publish(message, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}