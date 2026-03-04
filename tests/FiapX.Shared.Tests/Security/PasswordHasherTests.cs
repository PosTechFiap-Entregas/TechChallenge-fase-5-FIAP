using FiapX.Shared.Security;
using FluentAssertions;

namespace FiapX.Shared.Tests.Security;

public class PasswordHasherTests
{
    [Fact]
    public void HashPassword_WithValidPassword_ShouldReturnBCryptHash()
    {
        // Arrange
        var password = "MySecurePassword123!";

        // Act
        var hash = PasswordHasher.HashPassword(password);

        // Assert
        hash.Should().NotBeNullOrEmpty();
        hash.Should().StartWith("$2"); // BCrypt hash prefix
        hash.Should().NotBe(password);
        hash.Length.Should().Be(60); // BCrypt hash length
    }

    [Fact]
    public void HashPassword_WithSamePassword_ShouldGenerateDifferentHashes()
    {
        // Arrange
        var password = "SamePassword123!";

        // Act
        var hash1 = PasswordHasher.HashPassword(password);
        var hash2 = PasswordHasher.HashPassword(password);

        // Assert
        hash1.Should().NotBe(hash2); // BCrypt uses salt, so hashes are different
    }

    [Fact]
    public void VerifyPassword_WithCorrectPassword_ShouldReturnTrue()
    {
        // Arrange
        var password = "CorrectPassword123!";
        var hash = PasswordHasher.HashPassword(password);

        // Act
        var result = PasswordHasher.VerifyPassword(password, hash);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_WithIncorrectPassword_ShouldReturnFalse()
    {
        // Arrange
        var correctPassword = "CorrectPassword123!";
        var wrongPassword = "WrongPassword456!";
        var hash = PasswordHasher.HashPassword(correctPassword);

        // Act
        var result = PasswordHasher.VerifyPassword(wrongPassword, hash);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("", "validhash")]
    [InlineData("   ", "validhash")]
    [InlineData(null, "validhash")]
    [InlineData("validpassword", "")]
    [InlineData("validpassword", "   ")]
    [InlineData("validpassword", null)]
    public void VerifyPassword_WithInvalidInputs_ShouldReturnFalse(string password, string hash)
    {
        // Act
        var result = PasswordHasher.VerifyPassword(password, hash);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void HashPassword_WithInvalidPassword_ShouldThrowException(string password)
    {
        // Act
        var act = () => PasswordHasher.HashPassword(password);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Password cannot be empty*");
    }

    [Fact]
    public void VerifyPassword_WithInvalidHash_ShouldReturnFalse()
    {
        // Arrange
        var password = "Password123!";
        var invalidHash = "not-a-valid-bcrypt-hash";

        // Act
        var result = PasswordHasher.VerifyPassword(password, invalidHash);

        // Assert
        result.Should().BeFalse();
    }
}