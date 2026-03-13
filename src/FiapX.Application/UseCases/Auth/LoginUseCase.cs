using FiapX.Application.DTOs;
using FiapX.Application.Interfaces;
using FiapX.Application.Interfaces.UseCases;
using FiapX.Domain.Interfaces;
using FiapX.Shared.Results;
using FiapX.Shared.Security;

namespace FiapX.Application.UseCases.Auth;

public class LoginUseCase : ILoginUseCase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtTokenService _jwtService;

    public LoginUseCase(
        IUnitOfWork unitOfWork,
        IJwtTokenService jwtService)
    {
        _unitOfWork = unitOfWork;
        _jwtService = jwtService;
    }

    public async Task<Result<AuthResponse>> ExecuteAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users
            .GetByEmailAsync(request.Email, cancellationToken);

        if (user is null)
            return Result.Failure<AuthResponse>("Email ou senha inválidos.");

        if (!user.IsActive)
            return Result.Failure<AuthResponse>("Conta desativada.");

        var passwordValid = PasswordHasher.VerifyPassword(request.Password, user.PasswordHash);

        if (!passwordValid)
            return Result.Failure<AuthResponse>("Email ou senha inválidos.");

        var token = _jwtService.GenerateToken(user);

        var response = new AuthResponse
        {
            UserId = user.Id,
            Name = user.Name,
            Email = user.Email,
            Token = token,
            ExpiresAt = _jwtService.GetTokenExpiration()
        };

        return Result.Success(response);
    }
}