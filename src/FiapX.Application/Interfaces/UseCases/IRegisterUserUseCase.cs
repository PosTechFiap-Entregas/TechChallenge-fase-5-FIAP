using FiapX.Application.DTOs;
using FiapX.Shared.Results;

namespace FiapX.Application.Interfaces.UseCases
{
    /// <summary>
    /// Interface para o caso de uso de registro de usuário
    /// </summary>
    public interface IRegisterUserUseCase
    {
        Task<Result<AuthResponse>> ExecuteAsync(RegisterUserRequest request, CancellationToken cancellationToken = default);
    }
}