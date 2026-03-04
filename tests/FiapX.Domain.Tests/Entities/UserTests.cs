using FiapX.Domain.Entities;
using FluentAssertions;

namespace FiapX.Domain.Tests.Entities;

public class UserTests
{
    [Fact]
    public void Constructor_WithValidData_ShouldCreateUser()
    {
        // Arrange & Act
        var user = new User("test@example.com", "hash123", "John Doe");

        // Assert
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
        // Arrange & Act
        var user = new User("  Test@EXAMPLE.COM  ", "hash", "User");

        // Assert
        user.Email.Should().Be("test@example.com");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_WithInvalidEmail_ShouldThrowException(string email)
    {
        // Act
        var act = () => new User(email, "hash", "Name");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Email cannot be empty*");
    }

    [Fact]
    public void Constructor_WithEmailWithoutAt_ShouldThrowException()
    {
        // Act
        var act = () => new User("invalidemail.com", "hash", "Name");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Invalid email format*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_WithInvalidName_ShouldThrowException(string name)
    {
        // Act
        var act = () => new User("test@example.com", "hash", name);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Name cannot be empty*");
    }

    [Fact]
    public void UpdatePassword_WithValidHash_ShouldUpdatePassword()
    {
        // Arrange
        var user = new User("test@example.com", "oldhash", "User");
        var newHash = "newhash123";

        // Act
        user.UpdatePassword(newHash);

        // Assert
        user.PasswordHash.Should().Be(newHash);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void UpdatePassword_WithInvalidHash_ShouldThrowException(string hash)
    {
        // Arrange
        var user = new User("test@example.com", "hash", "User");

        // Act
        var act = () => user.UpdatePassword(hash);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Password hash cannot be empty*");
    }

    [Fact]
    public void UpdateName_WithValidName_ShouldUpdateName()
    {
        // Arrange
        var user = new User("test@example.com", "hash", "Old Name");

        // Act
        user.UpdateName("  New Name  ");

        // Assert
        user.Name.Should().Be("New Name");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void UpdateName_WithInvalidName_ShouldThrowException(string name)
    {
        // Arrange
        var user = new User("test@example.com", "hash", "User");

        // Act
        var act = () => user.UpdateName(name);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Name cannot be empty*");
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var user = new User("test@example.com", "hash", "User");
        user.IsActive.Should().BeTrue();

        // Act
        user.Deactivate();

        // Assert
        user.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Activate_AfterDeactivate_ShouldSetIsActiveToTrue()
    {
        // Arrange
        var user = new User("test@example.com", "hash", "User");
        user.Deactivate();
        user.IsActive.Should().BeFalse();

        // Act
        user.Activate();

        // Assert
        user.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Constructor_WithEmailTooLong_ShouldThrowException()
    {
        // Arrange
        var longEmail = new string('a', 250) + "@example.com"; // > 256 chars

        // Act
        var act = () => new User(longEmail, "hash", "User");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Email too long*");
    }

    [Fact]
    public void Constructor_WithNameTooLong_ShouldThrowException()
    {
        // Arrange
        var longName = new string('a', 201); // > 200 chars

        // Act
        var act = () => new User("test@example.com", "hash", longName);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Name too long*");
    }
}