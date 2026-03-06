using FiapX.Application.DTOs;
using FluentAssertions;

namespace FiapX.Application.Tests.DTOs;

public class RegisterUserRequestTests
{
    [Fact]
    public void RegisterUserRequest_DefaultValues_ShouldBeInitializedCorrectly()
    {
        var request = new RegisterUserRequest();

        request.Name.Should().Be(string.Empty);
        request.Email.Should().Be(string.Empty);
        request.Password.Should().Be(string.Empty);
        request.ConfirmPassword.Should().Be(string.Empty);
    }

    [Fact]
    public void RegisterUserRequest_WithAllPropertiesSet_ShouldReturnCorrectValues()
    {
        var request = new RegisterUserRequest
        {
            Name = "John Doe",
            Email = "john@example.com",
            Password = "P@ssw0rd!",
            ConfirmPassword = "P@ssw0rd!"
        };

        request.Name.Should().Be("John Doe");
        request.Email.Should().Be("john@example.com");
        request.Password.Should().Be("P@ssw0rd!");
        request.ConfirmPassword.Should().Be("P@ssw0rd!");
    }

    [Fact]
    public void RegisterUserRequest_PasswordAndConfirmPassword_ShouldBeIndependentProperties()
    {
        var request = new RegisterUserRequest
        {
            Password = "pass1",
            ConfirmPassword = "pass2"
        };

        request.Password.Should().Be("pass1");
        request.ConfirmPassword.Should().Be("pass2");
        request.Password.Should().NotBe(request.ConfirmPassword);
    }

    [Fact]
    public void RegisterUserRequest_PasswordAndConfirmPassword_WhenEqual_ShouldMatch()
    {
        var request = new RegisterUserRequest
        {
            Password = "SecurePass123",
            ConfirmPassword = "SecurePass123"
        };

        request.Password.Should().Be(request.ConfirmPassword);
    }

    [Fact]
    public void RegisterUserRequest_TwoInstancesWithSameValues_ShouldBeEqual()
    {
        var r1 = new RegisterUserRequest { Name = "A", Email = "a@b.com", Password = "p", ConfirmPassword = "p" };
        var r2 = new RegisterUserRequest { Name = "A", Email = "a@b.com", Password = "p", ConfirmPassword = "p" };

        r1.Should().Be(r2);
        (r1 == r2).Should().BeTrue();
    }

    [Fact]
    public void RegisterUserRequest_TwoInstancesWithDifferentName_ShouldNotBeEqual()
    {
        var r1 = new RegisterUserRequest { Name = "Alice" };
        var r2 = new RegisterUserRequest { Name = "Bob" };

        r1.Should().NotBe(r2);
    }

    [Fact]
    public void RegisterUserRequest_TwoInstancesWithDifferentEmail_ShouldNotBeEqual()
    {
        var r1 = new RegisterUserRequest { Email = "a@a.com" };
        var r2 = new RegisterUserRequest { Email = "b@b.com" };

        r1.Should().NotBe(r2);
    }

    [Fact]
    public void RegisterUserRequest_TwoInstancesWithDifferentConfirmPassword_ShouldNotBeEqual()
    {
        var r1 = new RegisterUserRequest { Password = "p", ConfirmPassword = "p" };
        var r2 = new RegisterUserRequest { Password = "p", ConfirmPassword = "different" };

        r1.Should().NotBe(r2);
    }

    [Fact]
    public void RegisterUserRequest_WithExpression_ShouldCreateNewInstanceWithUpdatedName()
    {
        var original = new RegisterUserRequest { Name = "Old Name", Email = "e@e.com" };

        var updated = original with { Name = "New Name" };

        updated.Name.Should().Be("New Name");
        updated.Email.Should().Be("e@e.com");
        original.Name.Should().Be("Old Name");
    }

    [Fact]
    public void RegisterUserRequest_TwoEqualInstances_ShouldHaveSameHashCode()
    {
        var r1 = new RegisterUserRequest { Name = "A", Email = "a@b.com", Password = "p", ConfirmPassword = "p" };
        var r2 = new RegisterUserRequest { Name = "A", Email = "a@b.com", Password = "p", ConfirmPassword = "p" };

        r1.GetHashCode().Should().Be(r2.GetHashCode());
    }

    [Fact]
    public void RegisterUserRequest_ToString_ShouldContainTypeName()
    {
        var request = new RegisterUserRequest { Name = "John" };

        var result = request.ToString();

        result.Should().Contain("RegisterUserRequest");
        result.Should().Contain("John");
    }
}
