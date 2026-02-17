using FiapX.Domain.Entities;

namespace FiapX.Domain.Interfaces;

/// <summary>
/// Interface do repositório de usuários
/// </summary>
public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
}
