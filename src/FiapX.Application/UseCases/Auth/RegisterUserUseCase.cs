using FiapX.Application.DTOs;
using FiapX.Application.Interfaces;
using FiapX.Domain.Entities;
using FiapX.Domain.Interfaces;
using FiapX.Shared.Results;
using FiapX.Shared.Security;

namespace FiapX.Application.UseCases.Auth;

/// <summary>
/// Use Case para registro de novo usuário
/// </summary>
public class RegisterUserUseCase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtTokenService _jwtService;

    public RegisterUserUseCase(
        IUnitOfWork unitOfWork,
        IJwtTokenService jwtService)
    {
        _unitOfWork = unitOfWork;
        _jwtService = jwtService;
    }

    public async Task<Result<AuthResponse>> ExecuteAsync(
        RegisterUserRequest request,
        CancellationToken cancellationToken = default)
    {
        // Verificar se email já existe
        var emailExists = await _unitOfWork.Users
            .EmailExistsAsync(request.Email, cancellationToken);

        if (emailExists)
            return Result.Failure<AuthResponse>("Email já está sendo utilizado.");

        // Criar hash da senha
        var passwordHash = PasswordHasher.HashPassword(request.Password);

        // Criar entidade
        var user = new User(request.Email, passwordHash, request.Name);

        // Persistir
        await _unitOfWork.Users.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Gerar token
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