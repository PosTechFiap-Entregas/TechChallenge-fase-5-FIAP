using FiapX.Application.Events;
using FiapX.Domain.Entities;
using FiapX.Domain.Enums;
using FiapX.Domain.Interfaces;
using FiapX.Worker.Services;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace FiapX.Worker.Consumers;

public class VideoUploadedEventConsumer : IConsumer<VideoUploadedEvent>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IStorageService _storageService;
    private readonly IVideoProcessingService _videoProcessingService;
    private readonly IMessagePublisher _messagePublisher;
    private readonly ITelegramNotificationService _telegramNotificationService;
    private readonly ILogger<VideoUploadedEventConsumer> _logger;
    private readonly VideoMetricsService _metrics;

    public VideoUploadedEventConsumer(
        IUnitOfWork unitOfWork,
        IStorageService storageService,
        IVideoProcessingService videoProcessingService,
        IMessagePublisher messagePublisher,
        ITelegramNotificationService telegramNotificationService,
        ILogger<VideoUploadedEventConsumer> logger,
        VideoMetricsService metrics)
    {
        _unitOfWork = unitOfWork;
        _storageService = storageService;
        _videoProcessingService = videoProcessingService;
        _messagePublisher = messagePublisher;
        _telegramNotificationService = telegramNotificationService;
        _logger = logger;
        _metrics = metrics;
    }

    public async Task Consume(ConsumeContext<VideoUploadedEvent> context)
    {
        var message = context.Message;
        var messageId = context.MessageId?.ToString() ?? Guid.NewGuid().ToString();

        _logger.LogInformation(
            "[{MessageId}] 🎬 Processando vídeo {VideoId} do usuário {UserId}",
            messageId,
            message.VideoId,
            message.UserId);

        var startTime = DateTime.UtcNow;
        _metrics.RecordVideoProcessingStarted();

        try
        {
            var video = await _unitOfWork.Videos.GetByIdAsync(message.VideoId, context.CancellationToken);

            if (video is null)
            {
                _logger.LogWarning(
                    "[{MessageId}] ⚠️ Vídeo {VideoId} não encontrado no banco de dados",
                    messageId,
                    message.VideoId);

                var duration = (DateTime.UtcNow - startTime).TotalSeconds;
                _metrics.RecordVideoProcessingFailed(duration);

                return;
            }

            if (video.Status == VideoStatus.Completed)
            {
                _logger.LogInformation(
                    "[{MessageId}] ✅ Vídeo {VideoId} já foi processado com sucesso. Ignorando mensagem duplicada.",
                    messageId,
                    message.VideoId);

                var duration = (DateTime.UtcNow - startTime).TotalSeconds;
                _metrics.RecordVideoProcessingFailed(duration);

                return;
            }

            if (video.Status == VideoStatus.Processing)
            {
                _logger.LogWarning(
                    "[{MessageId}] ⚙️ Vídeo {VideoId} já está sendo processado por outro worker. Ignorando.",
                    messageId,
                    message.VideoId);

                var duration = (DateTime.UtcNow - startTime).TotalSeconds;
                _metrics.RecordVideoProcessingFailed(duration);

                return;
            }

            var user = await _unitOfWork.Users.GetByIdAsync(video.UserId, context.CancellationToken);
            var userName = user?.Name ?? "Usuário desconhecido";

            video.StartProcessing();
            await _unitOfWork.SaveChangesAsync(context.CancellationToken);

            _logger.LogInformation(
                "[{MessageId}] 🔄 Vídeo {VideoId} marcado como processando",
                messageId,
                message.VideoId);

            var tempDirectory = _storageService.GetTempDirectory();
            var videoOutputDirectory = Path.Combine(tempDirectory, message.VideoId.ToString());
            Directory.CreateDirectory(videoOutputDirectory);

            _logger.LogDebug(
                "[{MessageId}] 📁 Diretório temporário criado: {Directory}",
                messageId,
                videoOutputDirectory);

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(context.CancellationToken);
            cts.CancelAfter(TimeSpan.FromMinutes(10));

            VideoProcessingResult result;
            try
            {
                _logger.LogInformation(
                    "[{MessageId}] ⚙️ Iniciando processamento do vídeo {VideoId}...",
                    messageId,
                    message.VideoId);

                result = await _videoProcessingService.ProcessVideoAsync(
                    message.StoragePath,
                    videoOutputDirectory,
                    fps: 1,
                    cts.Token);
            }
            catch (OperationCanceledException) when (cts.IsCancellationRequested && !context.CancellationToken.IsCancellationRequested)
            {
                _logger.LogError(
                    "[{MessageId}] ⏱️ Timeout no processamento do vídeo {VideoId} (>10 minutos)",
                    messageId,
                    message.VideoId);

                video.FailProcessing("Timeout: processamento excedeu 10 minutos");
                await _unitOfWork.SaveChangesAsync(context.CancellationToken);

                await _telegramNotificationService.NotifyVideoProcessingErrorAsync(
                    video.Id,
                    video.OriginalFileName,
                    userName,
                    "Timeout: processamento excedeu 10 minutos",
                    context.CancellationToken);

                var duration = (DateTime.UtcNow - startTime).TotalSeconds;
                _metrics.RecordVideoProcessingFailed(duration);

                throw;
            }

            if (result.Success)
            {
                _logger.LogInformation(
                    "[{MessageId}] 💾 Salvando ZIP do vídeo {VideoId}...",
                    messageId,
                    message.VideoId);

                var zipFileName = Path.GetFileName(result.ZipPath!);
                var zipStoragePath = await SaveZipToStorageAsync(
                    result.ZipPath!,
                    zipFileName,
                    context.CancellationToken);

                video.CompleteProcessing(
                    zipPath: zipStoragePath,
                    frameCount: result.FrameCount,
                    duration: result.ProcessingDuration);

                await _unitOfWork.SaveChangesAsync(context.CancellationToken);

                _metrics.RecordVideoProcessingSuccess(
                    result.ProcessingDuration.TotalSeconds,
                    result.FrameCount);

                _logger.LogInformation(
                    "[{MessageId}] ✅ Vídeo {VideoId} processado com sucesso: {FrameCount} frames em {Duration:F2}s",
                    messageId,
                    message.VideoId,
                    result.FrameCount,
                    result.ProcessingDuration.TotalSeconds);

                await _telegramNotificationService.NotifyVideoProcessingSuccessAsync(
                    video.Id,
                    video.OriginalFileName,
                    userName,
                    result.FrameCount,
                    result.ProcessingDuration,
                    context.CancellationToken);

                await _messagePublisher.PublishAsync(new VideoProcessedEvent
                {
                    VideoId = video.Id,
                    UserId = video.UserId,
                    Success = true,
                    FrameCount = result.FrameCount,
                    ZipPath = zipStoragePath,
                    ProcessingDuration = result.ProcessingDuration,
                    ProcessedAt = DateTime.UtcNow
                }, context.CancellationToken);
            }
            else
            {
                video.FailProcessing(result.ErrorMessage ?? "Erro desconhecido");
                await _unitOfWork.SaveChangesAsync(context.CancellationToken);

                _metrics.RecordVideoProcessingFailed(
                    result.ProcessingDuration.TotalSeconds);

                _logger.LogError(
                    "[{MessageId}] ❌ Falha ao processar vídeo {VideoId}: {Error}",
                    messageId,
                    message.VideoId,
                    result.ErrorMessage);

                await _telegramNotificationService.NotifyVideoProcessingErrorAsync(
                    video.Id,
                    video.OriginalFileName,
                    userName,
                    result.ErrorMessage ?? "Erro desconhecido",
                    context.CancellationToken);

                await _messagePublisher.PublishAsync(new VideoProcessedEvent
                {
                    VideoId = video.Id,
                    UserId = video.UserId,
                    Success = false,
                    ErrorMessage = result.ErrorMessage,
                    ProcessingDuration = result.ProcessingDuration,
                    ProcessedAt = DateTime.UtcNow
                }, context.CancellationToken);
            }

            if (Directory.Exists(videoOutputDirectory))
            {
                Directory.Delete(videoOutputDirectory, recursive: true);
                _logger.LogDebug(
                    "[{MessageId}] 🧹 Diretório temporário removido: {Directory}",
                    messageId,
                    videoOutputDirectory);
            }

            _logger.LogInformation(
                "[{MessageId}] 🎉 Processamento do vídeo {VideoId} finalizado",
                messageId,
                message.VideoId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "[{MessageId}] 💥 Erro crítico ao processar vídeo {VideoId}",
                messageId,
                message.VideoId);

            var duration = (DateTime.UtcNow - startTime).TotalSeconds;
            _metrics.RecordVideoProcessingFailed(duration);

            try
            {
                var video = await _unitOfWork.Videos.GetByIdAsync(message.VideoId, context.CancellationToken);
                if (video is not null && video.Status != VideoStatus.Completed)
                {
                    video.FailProcessing(ex.Message);
                    await _unitOfWork.SaveChangesAsync(context.CancellationToken);

                    var user = await _unitOfWork.Users.GetByIdAsync(video.UserId, context.CancellationToken);
                    var userName = user?.Name ?? "Usuário desconhecido";

                    await _telegramNotificationService.NotifyVideoProcessingErrorAsync(
                        video.Id,
                        video.OriginalFileName,
                        userName,
                        ex.Message,
                        context.CancellationToken);
                }
            }
            catch (Exception innerEx)
            {
                _logger.LogError(
                    innerEx,
                    "[{MessageId}] 💥 Erro ao marcar vídeo {VideoId} como falha",
                    messageId,
                    message.VideoId);
            }

            throw;
        }
    }

    private async Task<string> SaveZipToStorageAsync(
        string tempZipPath,
        string zipFileName,
        CancellationToken cancellationToken)
    {
        using var zipStream = File.OpenRead(tempZipPath);
        return await _storageService.SaveZipAsync(zipStream, zipFileName, cancellationToken);
    }
}