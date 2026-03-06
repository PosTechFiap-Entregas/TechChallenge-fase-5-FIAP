using FiapX.Application.DTOs;
using FluentAssertions;

namespace FiapX.Application.Tests.DTOs;

public class LoginRequestTests
{
    [Fact]
    public void LoginRequest_DefaultValues_ShouldBeInitializedCorrectly()
    {
        var request = new LoginRequest();

        request.Email.Should().Be(string.Empty);
        request.Password.Should().Be(string.Empty);
    }

    [Fact]
    public void LoginRequest_WithAllPropertiesSet_ShouldReturnCorrectValues()
    {
        var request = new LoginRequest
        {
            Email = "user@example.com",
            Password = "P@ssw0rd!"
        };

        request.Email.Should().Be("user@example.com");
        request.Password.Should().Be("P@ssw0rd!");
    }

    [Fact]
    public void LoginRequest_Email_ShouldAcceptEmailWithSubdomain()
    {
        var request = new LoginRequest { Email = "user@mail.example.com" };

        request.Email.Should().Be("user@mail.example.com");
    }

    [Fact]
    public void LoginRequest_Password_ShouldAcceptSpecialCharacters()
    {
        var request = new LoginRequest { Password = "!@#$%^&*()_+" };

        request.Password.Should().Be("!@#$%^&*()_+");
    }

    [Fact]
    public void LoginRequest_Password_ShouldAcceptEmptyString()
    {
        var request = new LoginRequest { Password = "" };

        request.Password.Should().BeEmpty();
    }

    [Fact]
    public void LoginRequest_TwoInstancesWithSameValues_ShouldBeEqual()
    {
        var r1 = new LoginRequest { Email = "a@b.com", Password = "pass" };
        var r2 = new LoginRequest { Email = "a@b.com", Password = "pass" };

        r1.Should().Be(r2);
        (r1 == r2).Should().BeTrue();
    }

    [Fact]
    public void LoginRequest_TwoInstancesWithDifferentEmail_ShouldNotBeEqual()
    {
        var r1 = new LoginRequest { Email = "a@b.com", Password = "pass" };
        var r2 = new LoginRequest { Email = "x@y.com", Password = "pass" };

        r1.Should().NotBe(r2);
    }

    [Fact]
    public void LoginRequest_TwoInstancesWithDifferentPassword_ShouldNotBeEqual()
    {
        var r1 = new LoginRequest { Email = "a@b.com", Password = "pass1" };
        var r2 = new LoginRequest { Email = "a@b.com", Password = "pass2" };

        r1.Should().NotBe(r2);
    }

    [Fact]
    public void LoginRequest_WithExpression_ShouldCreateNewInstanceWithUpdatedEmail()
    {
        var original = new LoginRequest { Email = "old@example.com", Password = "pass" };

        var updated = original with { Email = "new@example.com" };

        updated.Email.Should().Be("new@example.com");
        updated.Password.Should().Be("pass");
        original.Email.Should().Be("old@example.com");
    }

    [Fact]
    public void LoginRequest_WithExpression_ShouldCreateNewInstanceWithUpdatedPassword()
    {
        var original = new LoginRequest { Email = "user@example.com", Password = "old-pass" };

        var updated = original with { Password = "new-pass" };

        updated.Password.Should().Be("new-pass");
        original.Password.Should().Be("old-pass");
    }

    [Fact]
    public void LoginRequest_TwoEqualInstances_ShouldHaveSameHashCode()
    {
        var r1 = new LoginRequest { Email = "a@b.com", Password = "pass" };
        var r2 = new LoginRequest { Email = "a@b.com", Password = "pass" };

        r1.GetHashCode().Should().Be(r2.GetHashCode());
    }

    [Fact]
    public void LoginRequest_ToString_ShouldContainTypeName()
    {
        var request = new LoginRequest { Email = "user@example.com" };

        var result = request.ToString();

        result.Should().Contain("LoginRequest");
        result.Should().Contain("user@example.com");
    }
}
