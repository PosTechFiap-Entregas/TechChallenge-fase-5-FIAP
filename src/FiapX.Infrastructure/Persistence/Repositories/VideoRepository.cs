using FiapX.Domain.Entities;
using FiapX.Domain.Enums;
using FiapX.Domain.Interfaces;
using FiapX.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace FiapX.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementação do repositório de vídeos
/// </summary>
public class VideoRepository : Repository<Video>, IVideoRepository
{
    public VideoRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Video>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(v => v.UserId == userId)
            .OrderByDescending(v => v.UploadedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Video>> GetByStatusAsync(VideoStatus status, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(v => v.Status == status)
            .OrderBy(v => v.UploadedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Video>> GetPendingVideosAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(v => v.Status == VideoStatus.Queued || v.Status == VideoStatus.Processing)
            .OrderBy(v => v.UploadedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Video?> GetByIdWithUserAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(v => v.User)
            .FirstOrDefaultAsync(v => v.Id == id, cancellationToken);
    }

    public async Task<int> CountByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .CountAsync(v => v.UserId == userId, cancellationToken);
    }

    public async Task<int> CountByStatusAsync(VideoStatus status, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .CountAsync(v => v.Status == status, cancellationToken);
    }
}