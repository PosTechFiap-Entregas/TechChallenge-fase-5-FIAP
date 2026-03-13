using FiapX.Domain.Entities;
using FiapX.Infrastructure.Security;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;

namespace FiapX.Infrastructure.Tests.Security;

public class JwtTokenServiceTests
{
    private readonly IConfiguration _configuration;
    private readonly JwtTokenService _sut;

    public JwtTokenServiceTests()
    {
        var configData = new Dictionary<string, string>
        {
            { "JWT:Secret", "my-super-secret-key-for-testing-with-at-least-32-characters" },
            { "JWT:Issuer", "FiapX.Tests" },
            { "JWT:Audience", "FiapX.API" },
            { "JWT:ExpirationMinutes", "60" }
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData!)
            .Build();

        _sut = new JwtTokenService(_configuration);
    }

    [Fact]
    public void GenerateToken_WithValidUser_ShouldReturnValidJwtToken()
    {
        var user = new User("test@example.com", "hash", "Test User");

        var token = _sut.GenerateToken(user);

        token.Should().NotBeNullOrEmpty();

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        jwtToken.Should().NotBeNull();
        jwtToken.Issuer.Should().Be("FiapX.Tests");
        jwtToken.Audiences.Should().Contain("FiapX.API");
    }

    [Fact]
    public void GenerateToken_ShouldIncludeUserClaims()
    {
        var user = new User("user@test.com", "hash", "John Doe");

        var token = _sut.GenerateToken(user);

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        var claims = jwtToken.Claims.ToList();

        claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == user.Id.ToString());
        claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == "user@test.com");
        claims.Should().Contain(c => c.Type == "name" && c.Value == "John Doe");
        claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Jti);
    }

    [Fact]
    public void GenerateToken_ShouldSetCorrectExpiration()
    {
        var user = new User("test@example.com", "hash", "Test User");
        var beforeGeneration = DateTime.UtcNow;

        var token = _sut.GenerateToken(user);
        var afterGeneration = DateTime.UtcNow;

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        jwtToken.ValidTo.Should().BeAfter(beforeGeneration.AddMinutes(59));
        jwtToken.ValidTo.Should().BeBefore(afterGeneration.AddMinutes(61));
    }

    [Fact]
    public void GetTokenExpiration_ShouldReturn60MinutesFromNow()
    {
        var before = DateTimeOffset.UtcNow.AddMinutes(59);

        var expiration = _sut.GetTokenExpiration();

        var after = DateTimeOffset.UtcNow.AddMinutes(61);

        expiration.Should().BeAfter(before);
        expiration.Should().BeBefore(after);
    }

    [Fact]
    public void Constructor_WithMissingSecret_ShouldThrowException()
    {
        var configData = new Dictionary<string, string>
        {
            { "JWT:Issuer", "FiapX.Tests" },
            { "JWT:Audience", "FiapX.API" }
        };

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(configData!)
            .Build();

        var act = () => new JwtTokenService(config);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*JWT:Secret*");
    }

    [Fact]
    public void GenerateToken_WithDifferentUsers_ShouldGenerateDifferentTokens()
    {
        var user1 = new User("user1@test.com", "hash1", "User One");
        var user2 = new User("user2@test.com", "hash2", "User Two");

        var token1 = _sut.GenerateToken(user1);
        var token2 = _sut.GenerateToken(user2);

        token1.Should().NotBe(token2);

        var handler = new JwtSecurityTokenHandler();
        var jwtToken1 = handler.ReadJwtToken(token1);
        var jwtToken2 = handler.ReadJwtToken(token2);

        var sub1 = jwtToken1.Claims.First(c => c.Type == JwtRegisteredClaimNames.Sub).Value;
        var sub2 = jwtToken2.Claims.First(c => c.Type == JwtRegisteredClaimNames.Sub).Value;

        sub1.Should().NotBe(sub2);
    }
}