using FiapX.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Telegram.Bot;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace FiapX.Infrastructure.Tests.Services;

public class TelegramNotificationServiceTests
{
    private readonly Mock<ILogger<TelegramNotificationService>> _loggerMock;

    public TelegramNotificationServiceTests()
    {
        _loggerMock = new Mock<ILogger<TelegramNotificationService>>();
    }

    private static IConfiguration BuildConfig(
        string enabled = "false",
        string botToken = "",
        string chatId = "0")
    {
        var data = new Dictionary<string, string?>
        {
            ["Telegram:Enabled"] = enabled,
            ["Telegram:BotToken"] = botToken,
            ["Telegram:ChatId"] = chatId
        };
        return new ConfigurationBuilder().AddInMemoryCollection(data).Build();
    }

    private static Mock<ITelegramBotClient> CreateBotMock()
    {
        var mock = new Mock<ITelegramBotClient>();
        mock.Setup(b => b.SendRequest(It.IsAny<SendMessageRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Message { Id = 1 });
        return mock;
    }

    [Fact]
    public void Constructor_WithDisabledTelegram_ShouldLogDisabledMessage()
    {
        var config = BuildConfig(enabled: "false", botToken: "token", chatId: "123");
        var botMock = new Mock<ITelegramBotClient>();

        var service = new TelegramNotificationService(config, _loggerMock.Object, botMock.Object);

        service.Should().NotBeNull();
        _loggerMock.Verify(
            x => x.Log(LogLevel.Information, It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("desabilitadas")),
                null, It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void Constructor_WithValidConfigAndBotClient_ShouldLogInitialized()
    {
        var config = BuildConfig(enabled: "true", botToken: "test-token", chatId: "123456");
        var botMock = CreateBotMock();

        var service = new TelegramNotificationService(config, _loggerMock.Object, botMock.Object);

        service.Should().NotBeNull();
        _loggerMock.Verify(
            x => x.Log(LogLevel.Information, It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("Telegram Bot inicializado")),
                null, It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void Constructor_WithMissingBotToken_ShouldDisableService()
    {
        var config = BuildConfig(enabled: "true", botToken: "", chatId: "123456");

        var service = new TelegramNotificationService(config, _loggerMock.Object);

        service.Should().NotBeNull();
        _loggerMock.Verify(
            x => x.Log(LogLevel.Information, It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("desabilitadas")),
                null, It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void Constructor_WithMissingChatId_ShouldDisableService()
    {
        var config = BuildConfig(enabled: "true", botToken: "token", chatId: "0");

        var service = new TelegramNotificationService(config, _loggerMock.Object);

        service.Should().NotBeNull();
        _loggerMock.Verify(
            x => x.Log(LogLevel.Information, It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("desabilitadas")),
                null, It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void Constructor_WithValidConfigButNoBotClient_ShouldTryCreateBotClient()
    {
        var config = BuildConfig(enabled: "true", botToken: "valid-fake-token", chatId: "123456");

        var act = () => new TelegramNotificationService(config, _loggerMock.Object, null);

        act.Should().NotThrow();
    }

    [Fact]
    public void Constructor_WithNoConfiguration_ShouldNotThrow()
    {
        var config = BuildConfig();

        var act = () => new TelegramNotificationService(config, _loggerMock.Object);

        act.Should().NotThrow();
    }

    [Fact]
    public async Task NotifySuccess_WhenDisabled_ShouldLogDebugIgnored()
    {
        var config = BuildConfig(enabled: "false");
        var botMock = new Mock<ITelegramBotClient>();
        var service = new TelegramNotificationService(config, _loggerMock.Object, botMock.Object);

        await service.NotifyVideoProcessingSuccessAsync(
            Guid.NewGuid(), "test.mp4", "User", 100, TimeSpan.FromSeconds(10));

        _loggerMock.Verify(
            x => x.Log(LogLevel.Debug, It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("ignorada")),
                null, It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task NotifySuccess_WhenDisabled_ShouldNotThrow()
    {
        var config = BuildConfig(enabled: "false");
        var service = new TelegramNotificationService(config, _loggerMock.Object);

        var act = async () => await service.NotifyVideoProcessingSuccessAsync(
            Guid.NewGuid(), "test.mp4", "User", 100, TimeSpan.FromSeconds(10));

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task NotifySuccess_WhenEnabled_ShouldSendMessage()
    {
        var config = BuildConfig(enabled: "true", botToken: "token", chatId: "123456");
        var botMock = CreateBotMock();
        var service = new TelegramNotificationService(config, _loggerMock.Object, botMock.Object);

        await service.NotifyVideoProcessingSuccessAsync(
            Guid.NewGuid(), "video.mp4", "JoaoSilva", 50, TimeSpan.FromSeconds(5));

        botMock.Verify(
            b => b.SendRequest(It.IsAny<SendMessageRequest>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task NotifySuccess_WhenEnabled_ShouldLogSuccess()
    {
        var config = BuildConfig(enabled: "true", botToken: "token", chatId: "123456");
        var botMock = CreateBotMock();
        var service = new TelegramNotificationService(config, _loggerMock.Object, botMock.Object);
        var videoId = Guid.NewGuid();

        await service.NotifyVideoProcessingSuccessAsync(
            videoId, "video.mp4", "User", 30, TimeSpan.FromSeconds(2));

        _loggerMock.Verify(
            x => x.Log(LogLevel.Information, It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("Notificação de sucesso enviada")),
                null, It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task NotifySuccess_WhenEnabled_WithSpecialCharsInFileName_ShouldNotThrow()
    {
        var config = BuildConfig(enabled: "true", botToken: "token", chatId: "123456");
        var botMock = CreateBotMock();
        var service = new TelegramNotificationService(config, _loggerMock.Object, botMock.Object);

        var act = async () => await service.NotifyVideoProcessingSuccessAsync(
            Guid.NewGuid(),
            "video_test-final.mp4",
            "João (Silva) [Jr.]",
            100,
            TimeSpan.FromSeconds(10));

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task NotifySuccess_WhenSendMessageThrows_ShouldLogError()
    {
        var config = BuildConfig(enabled: "true", botToken: "token", chatId: "123456");
        var botMock = new Mock<ITelegramBotClient>();
        botMock.Setup(b => b.SendRequest(It.IsAny<SendMessageRequest>(), It.IsAny<CancellationToken>()))
               .ThrowsAsync(new Exception("Telegram API unavailable"));
        var service = new TelegramNotificationService(config, _loggerMock.Object, botMock.Object);

        var act = async () => await service.NotifyVideoProcessingSuccessAsync(
            Guid.NewGuid(), "video.mp4", "User", 10, TimeSpan.FromSeconds(1));

        await act.Should().NotThrowAsync();

        _loggerMock.Verify(
            x => x.Log(LogLevel.Error, It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("Erro ao enviar notificação de sucesso")),
                It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task NotifyError_WhenDisabled_ShouldLogDebugIgnored()
    {
        var config = BuildConfig(enabled: "false");
        var service = new TelegramNotificationService(config, _loggerMock.Object);

        await service.NotifyVideoProcessingErrorAsync(
            Guid.NewGuid(), "test.mp4", "User", "FFmpeg error");

        _loggerMock.Verify(
            x => x.Log(LogLevel.Debug, It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("ignorada")),
                null, It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task NotifyError_WhenDisabled_ShouldNotThrow()
    {
        var config = BuildConfig(enabled: "false");
        var service = new TelegramNotificationService(config, _loggerMock.Object);

        var act = async () => await service.NotifyVideoProcessingErrorAsync(
            Guid.NewGuid(), "test.mp4", "User", "some error");

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task NotifyError_WhenEnabled_ShouldSendMessage()
    {
        var config = BuildConfig(enabled: "true", botToken: "token", chatId: "123456");
        var botMock = CreateBotMock();
        var service = new TelegramNotificationService(config, _loggerMock.Object, botMock.Object);

        await service.NotifyVideoProcessingErrorAsync(
            Guid.NewGuid(), "video.mp4", "User", "Codec not supported");

        botMock.Verify(
            b => b.SendRequest(It.IsAny<SendMessageRequest>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task NotifyError_WhenEnabled_ShouldLogSuccess()
    {
        var config = BuildConfig(enabled: "true", botToken: "token", chatId: "123456");
        var botMock = CreateBotMock();
        var service = new TelegramNotificationService(config, _loggerMock.Object, botMock.Object);

        await service.NotifyVideoProcessingErrorAsync(
            Guid.NewGuid(), "video.mp4", "User", "error message");

        _loggerMock.Verify(
            x => x.Log(LogLevel.Information, It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("Notificação de erro enviada")),
                null, It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task NotifyError_WhenEnabled_WithSpecialCharsInErrorMessage_ShouldNotThrow()
    {
        var config = BuildConfig(enabled: "true", botToken: "token", chatId: "123456");
        var botMock = CreateBotMock();
        var service = new TelegramNotificationService(config, _loggerMock.Object, botMock.Object);

        var act = async () => await service.NotifyVideoProcessingErrorAsync(
            Guid.NewGuid(),
            "video.mp4",
            "User_Name",
            "Error: [codec] not found (exit_code=1)");

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task NotifyError_WhenSendMessageThrows_ShouldLogError()
    {
        var config = BuildConfig(enabled: "true", botToken: "token", chatId: "123456");
        var botMock = new Mock<ITelegramBotClient>();
        botMock.Setup(b => b.SendRequest(It.IsAny<SendMessageRequest>(), It.IsAny<CancellationToken>()))
               .ThrowsAsync(new Exception("Network error"));
        var service = new TelegramNotificationService(config, _loggerMock.Object, botMock.Object);

        var act = async () => await service.NotifyVideoProcessingErrorAsync(
            Guid.NewGuid(), "video.mp4", "User", "FFmpeg error");

        await act.Should().NotThrowAsync();

        _loggerMock.Verify(
            x => x.Log(LogLevel.Error, It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("Erro ao enviar notificação de erro")),
                It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task NotifyError_WhenEnabled_WithCancellationToken_ShouldNotThrow()
    {
        var config = BuildConfig(enabled: "true", botToken: "token", chatId: "123456");
        var botMock = CreateBotMock();
        var service = new TelegramNotificationService(config, _loggerMock.Object, botMock.Object);
        using var cts = new CancellationTokenSource();

        var act = async () => await service.NotifyVideoProcessingErrorAsync(
            Guid.NewGuid(), "video.mp4", "User", "error", cts.Token);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task NotifySuccess_WhenEnabled_WithCancellationToken_ShouldNotThrow()
    {
        var config = BuildConfig(enabled: "true", botToken: "token", chatId: "123456");
        var botMock = CreateBotMock();
        var service = new TelegramNotificationService(config, _loggerMock.Object, botMock.Object);
        using var cts = new CancellationTokenSource();

        var act = async () => await service.NotifyVideoProcessingSuccessAsync(
            Guid.NewGuid(), "video.mp4", "User", 10, TimeSpan.FromSeconds(1), cts.Token);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public void EscapeMarkdown_ShouldEscapeAllSpecialCharacters()
    {
        var method = typeof(TelegramNotificationService)
            .GetMethod("EscapeMarkdown",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        method.Should().NotBeNull();

        var input = "_ * [ ] ( ) ~ ` > # + - = | { } . !";
        var result = method!.Invoke(null, new object[] { input }) as string;

        result.Should().NotBeNull();
        result!.Should().Contain("\\_").And.Contain("\\*").And.Contain("\\[")
            .And.Contain("\\(").And.Contain("\\~").And.Contain("\\`")
            .And.Contain("\\>").And.Contain("\\#").And.Contain("\\+")
            .And.Contain("\\-").And.Contain("\\=").And.Contain("\\|")
            .And.Contain("\\{").And.Contain("\\}").And.Contain("\\.")
            .And.Contain("\\!");
    }

    [Fact]
    public void EscapeMarkdown_WithNull_ShouldReturnEmptyString()
    {
        var method = typeof(TelegramNotificationService)
            .GetMethod("EscapeMarkdown",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        method.Should().NotBeNull();

        var result = method!.Invoke(null, new object[] { null! }) as string;

        result.Should().Be(string.Empty);
    }

    [Fact]
    public void EscapeMarkdown_WithEmptyString_ShouldReturnEmptyString()
    {
        var method = typeof(TelegramNotificationService)
            .GetMethod("EscapeMarkdown",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        method.Should().NotBeNull();

        var result = method!.Invoke(null, new object[] { string.Empty }) as string;

        result.Should().Be(string.Empty);
    }

    [Fact]
    public void EscapeMarkdown_WithNormalText_ShouldReturnSameText()
    {
        var method = typeof(TelegramNotificationService)
            .GetMethod("EscapeMarkdown",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        method.Should().NotBeNull();

        const string input = "Hello World 123";
        var result = method!.Invoke(null, new object[] { input }) as string;

        result.Should().Be(input);
    }

    [Theory]
    [InlineData("_user_name_", "\\_user\\_name\\_")]
    [InlineData("*bold*", "\\*bold\\*")]
    [InlineData("[link](url)", "\\[link\\]\\(url\\)")]
    [InlineData("price: 10.00!", "price: 10\\.00\\!")]
    public void EscapeMarkdown_SpecificInputs_ShouldEscapeCorrectly(string input, string expected)
    {
        var method = typeof(TelegramNotificationService)
            .GetMethod("EscapeMarkdown",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        method.Should().NotBeNull();

        var result = method!.Invoke(null, new object[] { input }) as string;

        result.Should().Be(expected);
    }
}