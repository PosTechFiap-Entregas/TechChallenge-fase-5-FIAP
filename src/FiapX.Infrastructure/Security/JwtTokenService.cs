using FiapX.Application.Interfaces;
using FiapX.Domain.Entities;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace FiapX.Infrastructure.Security;

/// <summary>
/// Implementação do serviço de geração e validação de tokens JWT
/// </summary>
public class JwtTokenService : IJwtTokenService
{
    private readonly string _secret;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _expirationMinutes;

    public JwtTokenService(IConfiguration configuration)
    {
        _secret = configuration["JWT:Secret"]
            ?? throw new InvalidOperationException("JWT:Secret não configurado.");
        _issuer = configuration["JWT:Issuer"]
            ?? throw new InvalidOperationException("JWT:Issuer não configurado.");
        _audience = configuration["JWT:Audience"]
            ?? throw new InvalidOperationException("JWT:Audience não configurado.");
        _expirationMinutes = int.Parse(configuration["JWT:ExpirationMinutes"] ?? "60");
    }

    public string GenerateToken(User user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("name", user.Name),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()),
            new Claim(JwtRegisteredClaimNames.Exp, GetTokenExpiration().ToUnixTimeSeconds().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: GetTokenExpiration().UtcDateTime,
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public DateTimeOffset GetTokenExpiration()
    {
        return DateTimeOffset.UtcNow.AddMinutes(_expirationMinutes);
    }
}