namespace FiapX.Application.DTOs;

public record AuthResponse
{
    public Guid UserId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Token { get; init; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; init; }
}