using FiapX.Application.DTOs;
using FiapX.Shared.Results;

namespace FiapX.Application.Interfaces.UseCases
{
    public interface IDownloadVideoUseCase
    {
        Task<Result<VideoDownloadResponse>> ExecuteAsync(Guid videoId,Guid userId, CancellationToken cancellationToken = default);
    }
}