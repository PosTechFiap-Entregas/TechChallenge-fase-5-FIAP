using FiapX.Domain.Entities;
using FluentAssertions;

namespace FiapX.Domain.Tests.Entities;

public class UserTests
{
    [Fact]
    public void Constructor_WithValidData_ShouldCreateUser()
    {
        var user = new User("test@example.com", "hash123", "John Doe");

        user.Should().NotBeNull();
        user.Email.Should().Be("test@example.com");
        user.PasswordHash.Should().Be("hash123");
        user.Name.Should().Be("John Doe");
        user.IsActive.Should().BeTrue();
        user.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Constructor_ShouldNormalizeEmail()
    {
        var user = new User("  Test@EXAMPLE.COM  ", "hash", "User");

        user.Email.Should().Be("test@example.com");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_WithInvalidEmail_ShouldThrowException(string email)
    {
        var act = () => new User(email, "hash", "Name");

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Email cannot be empty*");
    }

    [Fact]
    public void Constructor_WithEmailWithoutAt_ShouldThrowException()
    {
        var act = () => new User("invalidemail.com", "hash", "Name");

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Invalid email format*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_WithInvalidName_ShouldThrowException(string name)
    {
        var act = () => new User("test@example.com", "hash", name);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Name cannot be empty*");
    }

    [Fact]
    public void UpdatePassword_WithValidHash_ShouldUpdatePassword()
    {
        var user = new User("test@example.com", "oldhash", "User");
        var newHash = "newhash123";

        user.UpdatePassword(newHash);

        user.PasswordHash.Should().Be(newHash);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void UpdatePassword_WithInvalidHash_ShouldThrowException(string hash)
    {
        var user = new User("test@example.com", "hash", "User");

        var act = () => user.UpdatePassword(hash);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Password hash cannot be empty*");
    }

    [Fact]
    public void UpdateName_WithValidName_ShouldUpdateName()
    {
        var user = new User("test@example.com", "hash", "Old Name");

        user.UpdateName("  New Name  ");

        user.Name.Should().Be("New Name");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void UpdateName_WithInvalidName_ShouldThrowException(string name)
    {
        var user = new User("test@example.com", "hash", "User");

        var act = () => user.UpdateName(name);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Name cannot be empty*");
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        var user = new User("test@example.com", "hash", "User");
        user.IsActive.Should().BeTrue();

        user.Deactivate();

        user.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Activate_AfterDeactivate_ShouldSetIsActiveToTrue()
    {
        var user = new User("test@example.com", "hash", "User");
        user.Deactivate();
        user.IsActive.Should().BeFalse();

        user.Activate();

        user.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Constructor_WithEmailTooLong_ShouldThrowException()
    {
        var longEmail = new string('a', 250) + "@example.com";

        var act = () => new User(longEmail, "hash", "User");

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Email too long*");
    }

    [Fact]
    public void Constructor_WithNameTooLong_ShouldThrowException()
    {
        var longName = new string('a', 201);

        var act = () => new User("test@example.com", "hash", longName);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Name too long*");
    }
}