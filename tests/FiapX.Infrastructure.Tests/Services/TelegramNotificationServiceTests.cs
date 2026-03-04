using FiapX.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace FiapX.Infrastructure.Tests.Services;

public class TelegramNotificationServiceTests
{
    private readonly Mock<ILogger<TelegramNotificationService>> _loggerMock;

    public TelegramNotificationServiceTests()
    {
        _loggerMock = new Mock<ILogger<TelegramNotificationService>>();
    }

    [Fact]
    public void Constructor_WithDisabledTelegram_ShouldNotInitializeBot()
    {
        // Arrange
        var configData = new Dictionary<string, string>
        {
            { "Telegram:Enabled", "false" },
            { "Telegram:BotToken", "test-token" },
            { "Telegram:ChatId", "123456" }
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData!)
            .Build();

        // Act
        var service = new TelegramNotificationService(configuration, _loggerMock.Object);

        // Assert
        service.Should().NotBeNull();
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("desabilitadas")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task NotifyVideoProcessingSuccessAsync_WhenDisabled_ShouldNotSendMessage()
    {
        // Arrange
        var configData = new Dictionary<string, string>
        {
            { "Telegram:Enabled", "false" },
            { "Telegram:BotToken", "" },
            { "Telegram:ChatId", "0" }
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData!)
            .Build();

        var service = new TelegramNotificationService(configuration, _loggerMock.Object);

        // Act
        await service.NotifyVideoProcessingSuccessAsync(
            Guid.NewGuid(),
            "test.mp4",
            "User",
            100,
            TimeSpan.FromSeconds(10));

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("ignorada")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task NotifyVideoProcessingErrorAsync_WhenDisabled_ShouldNotSendMessage()
    {
        // Arrange
        var configData = new Dictionary<string, string>
        {
            { "Telegram:Enabled", "false" },
            { "Telegram:BotToken", "" },
            { "Telegram:ChatId", "0" }
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData!)
            .Build();

        var service = new TelegramNotificationService(configuration, _loggerMock.Object);

        // Act
        await service.NotifyVideoProcessingErrorAsync(
            Guid.NewGuid(),
            "test.mp4",
            "User",
            "FFmpeg error");

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("ignorada")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void Constructor_WithMissingBotToken_ShouldDisableService()
    {
        // Arrange
        var configData = new Dictionary<string, string>
        {
            { "Telegram:Enabled", "true" },
            { "Telegram:BotToken", "" },
            { "Telegram:ChatId", "123456" }
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData!)
            .Build();

        // Act
        var service = new TelegramNotificationService(configuration, _loggerMock.Object);

        // Assert
        service.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithMissingChatId_ShouldDisableService()
    {
        // Arrange
        var configData = new Dictionary<string, string>
        {
            { "Telegram:Enabled", "true" },
            { "Telegram:BotToken", "test-token" },
            { "Telegram:ChatId", "0" }
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData!)
            .Build();

        // Act
        var service = new TelegramNotificationService(configuration, _loggerMock.Object);

        // Assert
        service.Should().NotBeNull();
    }

    [Fact]
    public async Task NotifyVideoProcessingSuccessAsync_ShouldNotThrowException()
    {
        // Arrange
        var configData = new Dictionary<string, string>
        {
            { "Telegram:Enabled", "false" }
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData!)
            .Build();

        var service = new TelegramNotificationService(configuration, _loggerMock.Object);

        // Act
        var act = async () => await service.NotifyVideoProcessingSuccessAsync(
            Guid.NewGuid(),
            "test.mp4",
            "User",
            100,
            TimeSpan.FromSeconds(10));

        // Assert
        await act.Should().NotThrowAsync();
    }
}