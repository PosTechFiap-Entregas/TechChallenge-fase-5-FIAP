using FiapX.Application.DTOs;
using FiapX.Shared.Results;

namespace FiapX.Application.Interfaces.UseCases
{
    public interface ILoginUseCase
    {
        Task<Result<AuthResponse>> ExecuteAsync(LoginRequest request, CancellationToken cancellationToken = default);
    }
}