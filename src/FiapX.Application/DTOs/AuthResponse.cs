namespace FiapX.Application.DTOs;

/// <summary>
/// DTO de resposta de autenticação (retorno do token)
/// </summary>
public record AuthResponse
{
    public Guid UserId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Token { get; init; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; init; }
}