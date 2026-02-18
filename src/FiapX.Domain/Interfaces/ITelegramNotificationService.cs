namespace FiapX.Domain.Interfaces;

/// <summary>
/// Serviço para envio de notificações via Telegram
/// </summary>
public interface ITelegramNotificationService
{
    /// <summary>
    /// Notifica sucesso no processamento do vídeo
    /// </summary>
    Task NotifyVideoProcessingSuccessAsync(
        Guid videoId,
        string fileName,
        string userName,
        int frameCount,
        TimeSpan duration,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Notifica falha no processamento do vídeo
    /// </summary>
    Task NotifyVideoProcessingErrorAsync(
        Guid videoId,
        string fileName,
        string userName,
        string errorMessage,
        CancellationToken cancellationToken = default);
}