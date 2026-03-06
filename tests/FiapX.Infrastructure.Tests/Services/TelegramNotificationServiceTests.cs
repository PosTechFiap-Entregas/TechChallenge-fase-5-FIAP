using FiapX.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Telegram.Bot;

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
        var configData = new Dictionary<string, string>
        {
            { "Telegram:Enabled", "false" },
            { "Telegram:BotToken", "test-token" },
            { "Telegram:ChatId", "123456" }
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData!)
            .Build();

        var botMock = new Mock<ITelegramBotClient>();
        var service = new TelegramNotificationService(configuration, _loggerMock.Object, botMock.Object);

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
        var configData = new Dictionary<string, string>
        {
            { "Telegram:Enabled", "false" },
            { "Telegram:BotToken", "" },
            { "Telegram:ChatId", "0" }
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData!)
            .Build();

        var botMock = new Mock<ITelegramBotClient>();
        var service = new TelegramNotificationService(configuration, _loggerMock.Object, botMock.Object);

        await service.NotifyVideoProcessingSuccessAsync(
            Guid.NewGuid(),
            "test.mp4",
            "User",
            100,
            TimeSpan.FromSeconds(10));

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

        await service.NotifyVideoProcessingErrorAsync(
            Guid.NewGuid(),
            "test.mp4",
            "User",
            "FFmpeg error");

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
        var configData = new Dictionary<string, string>
        {
            { "Telegram:Enabled", "true" },
            { "Telegram:BotToken", "" },
            { "Telegram:ChatId", "123456" }
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData!)
            .Build();

        var service = new TelegramNotificationService(configuration, _loggerMock.Object);

        service.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithValidConfig_ShouldInitializeBotAndLog()
    {
        var configData = new Dictionary<string, string>
        {
            { "Telegram:Enabled", "true" },
            { "Telegram:BotToken", "test-token" },
            { "Telegram:ChatId", "123456" }
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData!)
            .Build();

        var botMock = new Mock<ITelegramBotClient>();
        var service = new TelegramNotificationService(configuration, _loggerMock.Object, botMock.Object);

        service.Should().NotBeNull();

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Telegram Bot inicializado")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>() ),
            Times.Once);
    }

    [Fact]
    public void Constructor_WithMissingChatId_ShouldDisableService()
    {
        var configData = new Dictionary<string, string>
        {
            { "Telegram:Enabled", "true" },
            { "Telegram:BotToken", "test-token" },
            { "Telegram:ChatId", "0" }
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData!)
            .Build();

        var service = new TelegramNotificationService(configuration, _loggerMock.Object);

        service.Should().NotBeNull();
    }

    [Fact]
    public async Task NotifyVideoProcessingSuccessAsync_ShouldNotThrowException()
    {
        var configData = new Dictionary<string, string>
        {
            { "Telegram:Enabled", "false" }
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData!)
            .Build();

        var service = new TelegramNotificationService(configuration, _loggerMock.Object);

        var act = async () => await service.NotifyVideoProcessingSuccessAsync(
            Guid.NewGuid(),
            "test.mp4",
            "User",
            100,
            TimeSpan.FromSeconds(10));

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public void EscapeMarkdown_PrivateMethod_ShouldEscapeSpecialCharacters()
    {
        var type = typeof(TelegramNotificationService);
        var method = type.GetMethod("EscapeMarkdown", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        method.Should().NotBeNull();

        var input = "_ * [ ] ( ) ~ ` > # + - = | { } . !";

        var result = method!.Invoke(null, new object[] { input }) as string;

        result.Should().NotBeNull();
        result!.Should().Contain("\\_");
        result.Should().Contain("\\*");
        result.Should().Contain("\\[");
        result.Should().Contain("\\(");
        result.Should().Contain("\\~");
        result.Should().Contain("\\`");
        result.Should().Contain("\\>");
        result.Should().Contain("\\#");
        result.Should().Contain("\\+");
        result.Should().Contain("\\-");
        result.Should().Contain("\\=");
        result.Should().Contain("\\|");
        result.Should().Contain("\\{");
        result.Should().Contain("\\}");
        result.Should().Contain("\\.");
        result.Should().Contain("\\!");
    }

    [Fact]
    public void EscapeMarkdown_NullOrEmpty_ReturnsEmptyString()
    {
        var type = typeof(TelegramNotificationService);
        var method = type.GetMethod("EscapeMarkdown", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        method.Should().NotBeNull();

        var resultNull = method!.Invoke(null, new object[] { null }) as string;
        var resultEmpty = method.Invoke(null, new object[] { string.Empty }) as string;

        resultNull.Should().Be(string.Empty);
        resultEmpty.Should().Be(string.Empty);
    }
}