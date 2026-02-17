namespace FiapX.Application.DTOs;

/// <summary>
/// DTO para login de usuário
/// </summary>
public record LoginRequest
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}