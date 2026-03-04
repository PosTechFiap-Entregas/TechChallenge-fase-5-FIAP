using FiapX.Application.DTOs;
using FiapX.Shared.Results;

namespace FiapX.Application.Interfaces.UseCases
{
    /// <summary>
    /// Interface para o caso de uso de download de vídeo processado
    /// </summary>
    public interface IDownloadVideoUseCase
    {
        Task<Result<VideoDownloadResponse>> ExecuteAsync(Guid videoId,Guid userId, CancellationToken cancellationToken = default);
    }
}