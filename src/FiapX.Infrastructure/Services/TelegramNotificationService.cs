using FiapX.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace FiapX.Infrastructure.Services;

public class TelegramNotificationService : ITelegramNotificationService
{
    private readonly ILogger<TelegramNotificationService> _logger;
    private readonly bool _enabled;
    private readonly string? _botToken;
    private readonly long _chatId;
    private readonly ITelegramBotClient? _botClient;

    public TelegramNotificationService(
        IConfiguration configuration,
        ILogger<TelegramNotificationService> logger,
        ITelegramBotClient? botClient = null)
    {
        _logger = logger;
        _enabled = configuration.GetValue<bool>("Telegram:Enabled");
        _botToken = configuration["Telegram:BotToken"];

        var chatIdStr = configuration["Telegram:ChatId"];
        _chatId = !string.IsNullOrEmpty(chatIdStr) ? long.Parse(chatIdStr) : 0;

        if (botClient is not null && _enabled && !string.IsNullOrEmpty(_botToken) && _chatId != 0)
        {
            _botClient = botClient;
            _logger.LogInformation("Telegram Bot inicializado com sucesso. ChatId: {ChatId}", _chatId);
            return;
        }

        if (_enabled && !string.IsNullOrEmpty(_botToken) && _chatId != 0)
        {
            try
            {
                _botClient = new TelegramBotClient(_botToken);
                _logger.LogInformation("Telegram Bot inicializado com sucesso. ChatId: {ChatId}", _chatId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao inicializar Telegram Bot");
                _enabled = false;
            }
        }
        else
        {
            _logger.LogInformation("Notificações Telegram desabilitadas ou não configuradas");
        }
    }

    public async Task NotifyVideoProcessingSuccessAsync(
        Guid videoId,
        string fileName,
        string userName,
        int frameCount,
        TimeSpan duration,
        CancellationToken cancellationToken = default)
    {
        if (!_enabled || _botClient is null)
        {
            _logger.LogDebug("Notificação Telegram ignorada (desabilitada)");
            return;
        }

        try
        {
            var message = $"""
                ✅ *Vídeo Processado com Sucesso!*
                
                👤 *Usuário:* {EscapeMarkdown(userName)}
                📹 *Arquivo:* {EscapeMarkdown(fileName)}
                🆔 *ID:* `{videoId}`
                🎞️ *Frames extraídos:* {frameCount}
                ⏱️ *Tempo de processamento:* {duration.TotalSeconds:F1}s
                
                O arquivo ZIP com os frames está disponível para download!
                """;

            await _botClient.SendMessage(
                chatId: _chatId,
                text: message,
                parseMode: ParseMode.Markdown,
                cancellationToken: cancellationToken);

            _logger.LogInformation("Notificação de sucesso enviada para Telegram. VideoId: {VideoId}", videoId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar notificação de sucesso para Telegram. VideoId: {VideoId}", videoId);
        }
    }

    public async Task NotifyVideoProcessingErrorAsync(
        Guid videoId,
        string fileName,
        string userName,
        string errorMessage,
        CancellationToken cancellationToken = default)
    {
        if (!_enabled || _botClient is null)
        {
            _logger.LogDebug("Notificação Telegram ignorada (desabilitada)");
            return;
        }

        try
        {
            var message = $"""
                ❌ *Erro ao Processar Vídeo*
                
                👤 *Usuário:* {EscapeMarkdown(userName)}
                📹 *Arquivo:* {EscapeMarkdown(fileName)}
                🆔 *ID:* `{videoId}`
                ⚠️ *Erro:* {EscapeMarkdown(errorMessage)}
                
                Por favor, verifique o arquivo e tente novamente.
                """;

            await _botClient.SendMessage(
                chatId: _chatId,
                text: message,
                parseMode: ParseMode.Markdown,
                cancellationToken: cancellationToken);

            _logger.LogInformation("Notificação de erro enviada para Telegram. VideoId: {VideoId}", videoId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar notificação de erro para Telegram. VideoId: {VideoId}", videoId);
        }
    }

    private static string EscapeMarkdown(string text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        return text
            .Replace("_", "\\_")
            .Replace("*", "\\*")
            .Replace("[", "\\[")
            .Replace("]", "\\]")
            .Replace("(", "\\(")
            .Replace(")", "\\)")
            .Replace("~", "\\~")
            .Replace("`", "\\`")
            .Replace(">", "\\>")
            .Replace("#", "\\#")
            .Replace("+", "\\+")
            .Replace("-", "\\-")
            .Replace("=", "\\=")
            .Replace("|", "\\|")
            .Replace("{", "\\{")
            .Replace("}", "\\}")
            .Replace(".", "\\.")
            .Replace("!", "\\!");
    }
}