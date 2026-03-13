using FiapX.Application.DTOs;
using FiapX.Shared.Results;

namespace FiapX.Application.Interfaces.UseCases
{
    public interface IRegisterUserUseCase
    {
        Task<Result<AuthResponse>> ExecuteAsync(RegisterUserRequest request, CancellationToken cancellationToken = default);
    }
}