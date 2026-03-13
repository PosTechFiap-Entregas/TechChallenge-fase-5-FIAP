using FiapX.Application.DTOs;
using FiapX.Application.Interfaces.UseCases;
using FiapX.Domain.Interfaces;
using FiapX.Shared.Results;

namespace FiapX.Application.UseCases.Videos;

public class GetVideoStatusUseCase : IGetVideoStatusUseCase
{
    private readonly IUnitOfWork _unitOfWork;

    public GetVideoStatusUseCase(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<VideoStatusResponse>> ExecuteAsync(
        Guid videoId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var video = await _unitOfWork.Videos
            .GetByIdWithUserAsync(videoId, cancellationToken);

        if (video is null)
            return Result.Failure<VideoStatusResponse>("Vídeo não encontrado.");

        if (video.UserId != userId)
            return Result.Failure<VideoStatusResponse>("Acesso negado.");

        var response = new VideoStatusResponse
        {
            VideoId = video.Id,
            OriginalFileName = video.OriginalFileName,
            Status = video.Status.ToString(),
            StatusDescription = GetStatusDescription(video.Status),
            UploadedAt = video.UploadedAt,
            ProcessedAt = video.ProcessedAt,
            FrameCount = video.FrameCount,
            ErrorMessage = video.ErrorMessage,
            ProcessingDurationSeconds = video.ProcessingDuration?.TotalSeconds,
            CanDownload = video.CanDownload()
        };

        return Result.Success(response);
    }

    private static string GetStatusDescription(FiapX.Domain.Enums.VideoStatus status)
    {
        return status switch
        {
            FiapX.Domain.Enums.VideoStatus.Uploaded => "Enviado",
            FiapX.Domain.Enums.VideoStatus.Queued => "Na fila de processamento",
            FiapX.Domain.Enums.VideoStatus.Processing => "Processando frames...",
            FiapX.Domain.Enums.VideoStatus.Completed => "Processamento concluído",
            FiapX.Domain.Enums.VideoStatus.Failed => "Falha no processamento",
            _ => "Desconhecido"
        };
    }
}