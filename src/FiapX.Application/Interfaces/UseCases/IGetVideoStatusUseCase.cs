using FiapX.Application.DTOs;
using FiapX.Shared.Results;

namespace FiapX.Application.Interfaces.UseCases
{
    /// <summary>
    /// Interface para o caso de uso de consulta de status de vídeo
    /// </summary>
    public interface IGetVideoStatusUseCase
    {
        Task<Result<VideoStatusResponse>> ExecuteAsync(Guid videoId, Guid userId, CancellationToken cancellationToken = default);
    }
}