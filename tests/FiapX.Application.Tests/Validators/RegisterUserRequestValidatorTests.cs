using FiapX.Application.DTOs;
using FiapX.Application.Validators;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace FiapX.Application.Tests.Validators;

public class RegisterUserRequestValidatorTests
{
    private readonly RegisterUserRequestValidator _validator;

    public RegisterUserRequestValidatorTests()
    {
        _validator = new RegisterUserRequestValidator();
    }

    [Fact]
    public void Validate_WithValidData_ShouldNotHaveErrors()
    {
        var request = new RegisterUserRequest
        {
            Name = "John Doe",
            Email = "john@example.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        };

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Validate_WithEmptyName_ShouldHaveError(string name)
    {
        var request = new RegisterUserRequest
        {
            Name = name,
            Email = "john@example.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Nome é obrigatório.");
    }

    [Fact]
    public void Validate_WithShortName_ShouldHaveError()
    {
        var request = new RegisterUserRequest
        {
            Name = "J",
            Email = "john@example.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Validate_WithEmptyEmail_ShouldHaveError(string email)
    {
        var request = new RegisterUserRequest
        {
            Name = "John Doe",
            Email = email,
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email é obrigatório.");
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("user@")]
    [InlineData("@example.com")]
    public void Validate_WithInvalidEmail_ShouldHaveError(string email)
    {
        var request = new RegisterUserRequest
        {
            Name = "John Doe",
            Email = email,
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email inválido.");
    }

    [Fact]
    public void Validate_WithShortPassword_ShouldHaveError()
    {
        var request = new RegisterUserRequest
        {
            Name = "John Doe",
            Email = "john@example.com",
            Password = "123",
            ConfirmPassword = "123"
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Validate_WithEmptyPassword_ShouldHaveError(string password)
    {
        var request = new RegisterUserRequest
        {
            Name = "John Doe",
            Email = "john@example.com",
            Password = password,
            ConfirmPassword = "Password123!"
        };
        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Senha é obrigatória.");
    }

    [Fact]
    public void Validate_WithMismatchedPasswords_ShouldHaveError()
    {
        var request = new RegisterUserRequest
        {
            Name = "John Doe",
            Email = "john@example.com",
            Password = "Password123!",
            ConfirmPassword = "DifferentPassword123!"
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.ConfirmPassword)
            .WithErrorMessage("Senhas não coincidem.");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Validate_WithEmptyConfirmPassword_ShouldHaveError(string confirmPassword)
    {
        var request = new RegisterUserRequest
        {
            Name = "John Doe",
            Email = "john@example.com",
            Password = "Password123!",
            ConfirmPassword = confirmPassword
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.ConfirmPassword)
            .WithErrorMessage("Confirmação de senha é obrigatória.");
    }
}