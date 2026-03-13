using FiapX.Domain.Entities;

namespace FiapX.Application.Interfaces;

public interface IJwtTokenService
{
    string GenerateToken(User user);

    DateTimeOffset GetTokenExpiration();
}