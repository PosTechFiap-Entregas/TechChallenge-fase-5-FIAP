using FiapX.Domain.Entities;
using FiapX.Domain.Enums;

namespace FiapX.Domain.Interfaces;

/// <summary>
/// Interface do repositório de vídeos
/// </summary>
public interface IVideoRepository : IRepository<Video>
{
    Task<IEnumerable<Video>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Video>> GetByStatusAsync(VideoStatus status, CancellationToken cancellationToken = default);
    Task<IEnumerable<Video>> GetPendingVideosAsync(CancellationToken cancellationToken = default);
    Task<Video?> GetByIdWithUserAsync(Guid id, CancellationToken cancellationToken = default);
    Task<int> CountByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<int> CountByStatusAsync(VideoStatus status, CancellationToken cancellationToken = default);
}
