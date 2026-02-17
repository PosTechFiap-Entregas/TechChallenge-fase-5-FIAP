namespace FiapX.Domain.Interfaces;

/// <summary>
/// Interface para Unit of Work (gerenciamento de transações)
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IVideoRepository Videos { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitAsync(CancellationToken cancellationToken = default);
    Task RollbackAsync(CancellationToken cancellationToken = default);
}
