using FiapX.Application.DTOs;
using FiapX.Domain.Interfaces;
using FiapX.Shared.Results;

namespace FiapX.Application.UseCases.Videos;

/// <summary>
/// Use Case para listar vídeos do usuário autenticado
/// </summary>
public class GetUserVideosUseCase
{
    private readonly IUnitOfWork _unitOfWork;

    public GetUserVideosUseCase(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IEnumerable<VideoListResponse>>> ExecuteAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        // Verificar se o usuário existe
        var user = await _unitOfWork.Users
            .GetByIdAsync(userId, cancellationToken);

        if (user is null)
            return Result.Failure<IEnumerable<VideoListResponse>>("Usuário não encontrado.");

        // Buscar vídeos
        var videos = await _unitOfWork.Videos
            .GetByUserIdAsync(userId, cancellationToken);

        var response = videos.Select(v => new VideoListResponse
        {
            VideoId = v.Id,
            OriginalFileName = v.OriginalFileName,
            FileSizeMB = Math.Round(v.FileSizeBytes / (1024.0 * 1024.0), 2),
            Status = v.Status.ToString(),
            StatusDescription = GetStatusDescription(v.Status),
            UploadedAt = v.UploadedAt,
            ProcessedAt = v.ProcessedAt,
            FrameCount = v.FrameCount,
            CanDownload = v.CanDownload()
        });

        return Result.Success(response);
    }

    private static string GetStatusDescription(FiapX.Domain.Enums.VideoStatus status)
    {
        return status switch
        {
            FiapX.Domain.Enums.VideoStatus.Uploaded => "Enviado",
            FiapX.Domain.Enums.VideoStatus.Queued => "Na fila",
            FiapX.Domain.Enums.VideoStatus.Processing => "Processando",
            FiapX.Domain.Enums.VideoStatus.Completed => "Concluído",
            FiapX.Domain.Enums.VideoStatus.Failed => "Falha",
            _ => "Desconhecido"
        };
    }
}