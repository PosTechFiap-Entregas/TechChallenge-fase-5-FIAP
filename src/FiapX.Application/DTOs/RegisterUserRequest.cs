namespace FiapX.Application.DTOs;

/// <summary>
/// DTO para registro de novo usuário
/// </summary>
public record RegisterUserRequest
{
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string ConfirmPassword { get; init; } = string.Empty;
}