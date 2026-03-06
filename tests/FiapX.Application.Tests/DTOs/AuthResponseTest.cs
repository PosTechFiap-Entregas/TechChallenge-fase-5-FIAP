using FiapX.Application.DTOs;
using FluentAssertions;

namespace FiapX.Application.Tests.DTOs;

public class AuthResponseTests
{
    [Fact]
    public void AuthResponse_DefaultValues_ShouldBeInitializedCorrectly()
    {
        var response = new AuthResponse();

        response.UserId.Should().Be(Guid.Empty);
        response.Name.Should().Be(string.Empty);
        response.Email.Should().Be(string.Empty);
        response.Token.Should().Be(string.Empty);
        response.ExpiresAt.Should().Be(default(DateTimeOffset));
    }

    [Fact]
    public void AuthResponse_WithAllPropertiesSet_ShouldReturnCorrectValues()
    {
        var userId = Guid.NewGuid();
        var expiresAt = DateTimeOffset.UtcNow.AddHours(1);

        var response = new AuthResponse
        {
            UserId = userId,
            Name = "John Doe",
            Email = "john@example.com",
            Token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9",
            ExpiresAt = expiresAt
        };

        response.UserId.Should().Be(userId);
        response.Name.Should().Be("John Doe");
        response.Email.Should().Be("john@example.com");
        response.Token.Should().Be("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9");
        response.ExpiresAt.Should().Be(expiresAt);
    }

    [Fact]
    public void AuthResponse_Properties_ShouldBeInitOnly()
    {
        var response = new AuthResponse { Name = "Jane" };

        response.Name.Should().Be("Jane");
    }

    [Fact]
    public void AuthResponse_TwoInstancesWithSameValues_ShouldBeEqual()
    {
        var userId = Guid.NewGuid();
        var expiresAt = DateTimeOffset.UtcNow.AddHours(1);

        var response1 = new AuthResponse
        {
            UserId = userId,
            Name = "John",
            Email = "john@example.com",
            Token = "token123",
            ExpiresAt = expiresAt
        };

        var response2 = new AuthResponse
        {
            UserId = userId,
            Name = "John",
            Email = "john@example.com",
            Token = "token123",
            ExpiresAt = expiresAt
        };

        response1.Should().Be(response2);
        (response1 == response2).Should().BeTrue();
    }

    [Fact]
    public void AuthResponse_TwoInstancesWithDifferentValues_ShouldNotBeEqual()
    {
        var response1 = new AuthResponse { Name = "John", Token = "token-a" };
        var response2 = new AuthResponse { Name = "Jane", Token = "token-b" };

        response1.Should().NotBe(response2);
        (response1 != response2).Should().BeTrue();
    }

    [Fact]
    public void AuthResponse_WithExpression_ShouldCreateNewInstanceWithUpdatedValue()
    {
        var original = new AuthResponse { Name = "John", Email = "john@example.com", Token = "old-token" };

        var updated = original with { Token = "new-token" };

        updated.Token.Should().Be("new-token");
        updated.Name.Should().Be("John");
        updated.Email.Should().Be("john@example.com");
        original.Token.Should().Be("old-token");
    }

    [Fact]
    public void AuthResponse_UserId_ShouldAcceptValidGuid()
    {
        var guid = Guid.NewGuid();

        var response = new AuthResponse { UserId = guid };

        response.UserId.Should().NotBe(Guid.Empty);
        response.UserId.Should().Be(guid);
    }

    [Fact]
    public void AuthResponse_ExpiresAt_ShouldAcceptFutureDate()
    {
        var futureDate = DateTimeOffset.UtcNow.AddDays(30);

        var response = new AuthResponse { ExpiresAt = futureDate };

        response.ExpiresAt.Should().BeAfter(DateTimeOffset.UtcNow);
    }

    [Fact]
    public void AuthResponse_ExpiresAt_ShouldAcceptPastDate()
    {
        var pastDate = DateTimeOffset.UtcNow.AddDays(-1);

        var response = new AuthResponse { ExpiresAt = pastDate };

        response.ExpiresAt.Should().BeBefore(DateTimeOffset.UtcNow);
    }

    [Fact]
    public void AuthResponse_ToString_ShouldContainTypeName()
    {
        var response = new AuthResponse { Name = "John" };

        var result = response.ToString();

        result.Should().Contain("AuthResponse");
        result.Should().Contain("John");
    }

    [Fact]
    public void AuthResponse_TwoEqualInstances_ShouldHaveSameHashCode()
    {
        var userId = Guid.NewGuid();
        var expiresAt = DateTimeOffset.UtcNow;

        var r1 = new AuthResponse { UserId = userId, Name = "A", Email = "a@b.com", Token = "t", ExpiresAt = expiresAt };
        var r2 = new AuthResponse { UserId = userId, Name = "A", Email = "a@b.com", Token = "t", ExpiresAt = expiresAt };

        r1.GetHashCode().Should().Be(r2.GetHashCode());
    }
}
