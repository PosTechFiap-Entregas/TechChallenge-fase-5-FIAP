using FiapX.Application.Events;
using FiapX.Domain.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace FiapX.Worker.Consumers;

/// <summary>
/// Consumer que processa vídeos quando eles são enviados.
/// Consome VideoUploadedEvent da fila RabbitMQ.
/// </summary>
public class VideoUploadedEventConsumer : IConsumer<VideoUploadedEvent>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IStorageService _storageService;
    private readonly IVideoProcessingService _videoProcessingService;
    private readonly IMessagePublisher _messagePublisher;
    private readonly ITelegramNotificationService _telegramNotificationService;
    private readonly ILogger<VideoUploadedEventConsumer> _logger;

    public VideoUploadedEventConsumer(
        IUnitOfWork unitOfWork,
        IStorageService storageService,
        IVideoProcessingService videoProcessingService,
        IMessagePublisher messagePublisher,
        ITelegramNotificationService telegramNotificationService,
        ILogger<VideoUploadedEventConsumer> logger)
    {
        _unitOfWork = unitOfWork;
        _storageService = storageService;
        _videoProcessingService = videoProcessingService;
        _messagePublisher = messagePublisher;
        _telegramNotificationService = telegramNotificationService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<VideoUploadedEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Processando vídeo {VideoId} do usuário {UserId}",
            message.VideoId,
            message.UserId);

        try
        {
            // Buscar vídeo no banco
            var video = await _unitOfWork.Videos.GetByIdAsync(message.VideoId, context.CancellationToken);

            if (video is null)
            {
                _logger.LogWarning("Vídeo {VideoId} não encontrado no banco de dados", message.VideoId);
                return;
            }

            // Buscar usuário no banco para pegar o nome
            var user = await _unitOfWork.Users.GetByIdAsync(video.UserId, context.CancellationToken);
            var userName = user?.Name ?? "Usuário desconhecido";

            // Marcar como processando
            video.StartProcessing();
            await _unitOfWork.SaveChangesAsync(context.CancellationToken);

            _logger.LogInformation("Vídeo {VideoId} marcado como processando", message.VideoId);

            // Obter diretório temporário para processamento
            var tempDirectory = _storageService.GetTempDirectory();
            var videoOutputDirectory = Path.Combine(tempDirectory, message.VideoId.ToString());
            Directory.CreateDirectory(videoOutputDirectory);

            // Processar vídeo (extrair frames e criar ZIP)
            var result = await _videoProcessingService.ProcessVideoAsync(
                message.StoragePath,
                videoOutputDirectory,
                fps: 1, // 1 frame por segundo
                context.CancellationToken);

            if (result.Success)
            {
                // Salvar ZIP no storage permanente
                var zipFileName = Path.GetFileName(result.ZipPath!);
                var zipStoragePath = await SaveZipToStorageAsync(
                    result.ZipPath!,
                    zipFileName,
                    context.CancellationToken);

                // Marcar como concluído
                video.CompleteProcessing(
                    zipPath: zipStoragePath,
                    frameCount: result.FrameCount,
                    duration: result.ProcessingDuration);

                await _unitOfWork.SaveChangesAsync(context.CancellationToken);

                _logger.LogInformation(
                    "Vídeo {VideoId} processado com sucesso: {FrameCount} frames em {Duration}s",
                    message.VideoId,
                    result.FrameCount,
                    result.ProcessingDuration.TotalSeconds);

                // Notificar sucesso via Telegram (com nome do usuário)
                await _telegramNotificationService.NotifyVideoProcessingSuccessAsync(
                    video.Id,
                    video.OriginalFileName,
                    userName,
                    result.FrameCount,
                    result.ProcessingDuration,
                    context.CancellationToken);

                // Publicar evento de conclusão
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
                // Marcar como falha
                video.FailProcessing(result.ErrorMessage ?? "Erro desconhecido");
                await _unitOfWork.SaveChangesAsync(context.CancellationToken);

                _logger.LogError(
                    "Falha ao processar vídeo {VideoId}: {Error}",
                    message.VideoId,
                    result.ErrorMessage);

                // Notificar erro via Telegram (com nome do usuário)
                await _telegramNotificationService.NotifyVideoProcessingErrorAsync(
                    video.Id,
                    video.OriginalFileName,
                    userName,
                    result.ErrorMessage ?? "Erro desconhecido",
                    context.CancellationToken);

                // Publicar evento de falha
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

            // Limpar diretório temporário
            if (Directory.Exists(videoOutputDirectory))
                Directory.Delete(videoOutputDirectory, recursive: true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro crítico ao processar vídeo {VideoId}", message.VideoId);

            // Tentar marcar como falha no banco
            try
            {
                var video = await _unitOfWork.Videos.GetByIdAsync(message.VideoId, context.CancellationToken);
                if (video is not null)
                {
                    video.FailProcessing(ex.Message);
                    await _unitOfWork.SaveChangesAsync(context.CancellationToken);

                    // Buscar nome do usuário
                    var user = await _unitOfWork.Users.GetByIdAsync(video.UserId, context.CancellationToken);
                    var userName = user?.Name ?? "Usuário desconhecido";

                    // Notificar erro crítico via Telegram (com nome do usuário)
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
                _logger.LogError(innerEx, "Erro ao marcar vídeo {VideoId} como falha", message.VideoId);
            }

            throw; // Re-throw para que MassTransit faça retry
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