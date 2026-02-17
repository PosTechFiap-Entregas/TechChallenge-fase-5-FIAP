using FiapX.Application.DTOs;
using FiapX.Domain.Interfaces;
using FiapX.Shared.Results;

namespace FiapX.Application.UseCases.Videos;

/// <summary>
/// Use Case para download do ZIP de frames processados
/// </summary>
public class DownloadVideoUseCase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IStorageService _storageService;

    public DownloadVideoUseCase(
        IUnitOfWork unitOfWork,
        IStorageService storageService)
    {
        _unitOfWork = unitOfWork;
        _storageService = storageService;
    }

    public async Task<Result<VideoDownloadResponse>> ExecuteAsync(
        Guid videoId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        // Buscar vídeo
        var video = await _unitOfWork.Videos
            .GetByIdWithUserAsync(videoId, cancellationToken);

        if (video is null)
            return Result.Failure<VideoDownloadResponse>("Vídeo não encontrado.");

        // Validar ownership
        if (video.UserId != userId)
            return Result.Failure<VideoDownloadResponse>("Acesso negado.");

        // Verificar se pode fazer download
        if (!video.CanDownload())
            return Result.Failure<VideoDownloadResponse>("Vídeo ainda não foi processado ou falhou no processamento.");

        // Verificar se o arquivo ZIP existe
        var fileExists = await _storageService
            .FileExistsAsync(video.ZipPath!, cancellationToken);

        if (!fileExists)
            return Result.Failure<VideoDownloadResponse>("Arquivo ZIP não encontrado no storage.");

        // Obter stream do arquivo
        var stream = await _storageService
            .GetFileAsync(video.ZipPath!, cancellationToken);

        var fileSize = await _storageService
            .GetFileSizeAsync(video.ZipPath!, cancellationToken);

        var fileName = Path.GetFileName(video.ZipPath!);

        var response = new VideoDownloadResponse
        {
            FileStream = stream,
            FileName = fileName,
            FileSize = fileSize
        };

        return Result.Success(response);
    }
}