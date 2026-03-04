using FiapX.Application.DTOs;
using FiapX.Shared.Results;

namespace FiapX.Application.Interfaces.UseCases
{
    /// <summary>
    /// Interface para o caso de uso de login
    /// </summary>
    public interface ILoginUseCase
    {
        Task<Result<AuthResponse>> ExecuteAsync(LoginRequest request, CancellationToken cancellationToken = default);
    }
}