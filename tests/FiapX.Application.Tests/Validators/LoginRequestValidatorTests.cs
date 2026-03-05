using FiapX.Application.DTOs;
using FiapX.Application.Validators;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace FiapX.Application.Tests.Validators;

public class LoginRequestValidatorTests
{
    private readonly LoginRequestValidator _validator;

    public LoginRequestValidatorTests()
    {
        _validator = new LoginRequestValidator();
    }

    [Fact]
    public void Validate_WithValidData_ShouldNotHaveErrors()
    {
        var request = new LoginRequest
        {
            Email = "user@example.com",
            Password = "Password123!"
        };

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Validate_WithEmptyEmail_ShouldHaveError(string email)
    {
        var request = new LoginRequest
        {
            Email = email,
            Password = "Password123!"
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email é obrigatório.");
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("user@")]
    [InlineData("@example.com")]
    [InlineData("user.example.com")]
    public void Validate_WithInvalidEmail_ShouldHaveError(string email)
    {
        var request = new LoginRequest
        {
            Email = email,
            Password = "Password123!"
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email inválido.");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Validate_WithEmptyPassword_ShouldHaveError(string password)
    {
        var request = new LoginRequest
        {
            Email = "user@example.com",
            Password = password
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Senha é obrigatória.");
    }

    [Fact]
    public void Validate_WithMultipleErrors_ShouldHaveAllErrors()
    {
        var request = new LoginRequest
        {
            Email = "invalid-email",
            Password = ""
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Email);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }
}