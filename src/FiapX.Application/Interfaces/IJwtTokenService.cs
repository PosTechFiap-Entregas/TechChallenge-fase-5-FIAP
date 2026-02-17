using FiapX.Domain.Entities;

namespace FiapX.Application.Interfaces;

/// <summary>
/// Interface para o serviço de geração e validação de tokens JWT
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Gera um token JWT para o usuário
    /// </summary>
    string GenerateToken(User user);

    /// <summary>
    /// Retorna a data de expiração do token
    /// </summary>
    DateTimeOffset GetTokenExpiration();
}