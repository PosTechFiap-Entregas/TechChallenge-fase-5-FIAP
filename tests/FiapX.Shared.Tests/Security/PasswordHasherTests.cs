using FiapX.Shared.Security;
using FluentAssertions;

namespace FiapX.Shared.Tests.Security;

public class PasswordHasherTests
{
    [Fact]
    public void HashPassword_WithValidPassword_ShouldReturnBCryptHash()
    {
        var password = "MySecurePassword123!";

        var hash = PasswordHasher.HashPassword(password);

        hash.Should().NotBeNullOrEmpty();
        hash.Should().StartWith("$2");
        hash.Should().NotBe(password);
        hash.Length.Should().Be(60);
    }

    [Fact]
    public void HashPassword_WithSamePassword_ShouldGenerateDifferentHashes()
    {
        var password = "SamePassword123!";

        var hash1 = PasswordHasher.HashPassword(password);
        var hash2 = PasswordHasher.HashPassword(password);

        hash1.Should().NotBe(hash2);
    }

    [Fact]
    public void VerifyPassword_WithCorrectPassword_ShouldReturnTrue()
    {
        var password = "CorrectPassword123!";
        var hash = PasswordHasher.HashPassword(password);

        var result = PasswordHasher.VerifyPassword(password, hash);

        result.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_WithIncorrectPassword_ShouldReturnFalse()
    {
        var correctPassword = "CorrectPassword123!";
        var wrongPassword = "WrongPassword456!";
        var hash = PasswordHasher.HashPassword(correctPassword);

        var result = PasswordHasher.VerifyPassword(wrongPassword, hash);

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
        var result = PasswordHasher.VerifyPassword(password, hash);

        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void HashPassword_WithInvalidPassword_ShouldThrowException(string password)
    {
        var act = () => PasswordHasher.HashPassword(password);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Password cannot be empty*");
    }

    [Fact]
    public void VerifyPassword_WithInvalidHash_ShouldReturnFalse()
    {
        var password = "Password123!";
        var invalidHash = "not-a-valid-bcrypt-hash";

        var result = PasswordHasher.VerifyPassword(password, invalidHash);

        result.Should().BeFalse();
    }
}