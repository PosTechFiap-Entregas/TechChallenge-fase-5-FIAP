using FiapX.Application.DTOs;
using FiapX.Shared.Results;

namespace FiapX.Application.Interfaces.UseCases
{
    public interface IGetVideoStatusUseCase
    {
        Task<Result<VideoStatusResponse>> ExecuteAsync(Guid videoId, Guid userId, CancellationToken cancellationToken = default);
    }
}