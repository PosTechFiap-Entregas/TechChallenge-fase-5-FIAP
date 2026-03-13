namespace FiapX.Domain.Interfaces;

public interface ITelegramNotificationService
{
    Task NotifyVideoProcessingSuccessAsync(
        Guid videoId,
        string fileName,
        string userName,
        int frameCount,
        TimeSpan duration,
        CancellationToken cancellationToken = default);

    Task NotifyVideoProcessingErrorAsync(
        Guid videoId,
        string fileName,
        string userName,
        string errorMessage,
        CancellationToken cancellationToken = default);
}