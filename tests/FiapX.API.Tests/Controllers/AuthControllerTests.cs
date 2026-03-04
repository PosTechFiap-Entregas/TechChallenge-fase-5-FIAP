using FiapX.API.Controllers;
using FiapX.Application.DTOs;
using FiapX.Application.Interfaces.UseCases;
using FiapX.Shared.Results;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FiapX.API.Tests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IRegisterUserUseCase> _registerUseCaseMock;
    private readonly Mock<ILoginUseCase> _loginUseCaseMock;
    private readonly Mock<IValidator<RegisterUserRequest>> _registerValidatorMock;
    private readonly Mock<IValidator<LoginRequest>> _loginValidatorMock;
    private readonly AuthController _sut;

    public AuthControllerTests()
    {
        _registerUseCaseMock = new Mock<IRegisterUserUseCase>();
        _loginUseCaseMock = new Mock<ILoginUseCase>();
        _registerValidatorMock = new Mock<IValidator<RegisterUserRequest>>();
        _loginValidatorMock = new Mock<IValidator<LoginRequest>>();

        _sut = new AuthController(
            _registerUseCaseMock.Object,
            _loginUseCaseMock.Object,
            _registerValidatorMock.Object,
            _loginValidatorMock.Object);
    }

    [Fact]
    public async Task Register_WithValidData_ShouldReturnCreated()
    {
        var request = new RegisterUserRequest
        {
            Email = "test@example.com",
            Password = "Password123!",
            Name = "Test User"
        };

        var authResponse = new AuthResponse
        {
            UserId = Guid.NewGuid(),
            Email = request.Email,
            Name = request.Name,
            Token = "jwt-token",
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(1)
        };

        _registerValidatorMock
            .Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _registerUseCaseMock
            .Setup(x => x.ExecuteAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(authResponse));

        var result = await _sut.Register(request, CancellationToken.None);

        result.Should().BeOfType<CreatedResult>();
    }

    [Fact]
    public async Task Register_WithInvalidData_ShouldReturnBadRequest()
    {
        var request = new RegisterUserRequest
        {
            Email = "invalid-email",
            Password = "123",
            Name = ""
        };

        var validationFailures = new List<ValidationFailure>
        {
            new("Email", "Email inválido"),
            new("Password", "Senha muito curta")
        };

        _registerValidatorMock
            .Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(validationFailures));

        var result = await _sut.Register(request, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnOk()
    {
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "Password123!"
        };

        var authResponse = new AuthResponse
        {
            UserId = Guid.NewGuid(),
            Email = request.Email,
            Name = "Test User",
            Token = "jwt-token",
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(1)
        };

        _loginValidatorMock
            .Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _loginUseCaseMock
            .Setup(x => x.ExecuteAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(authResponse));

        var result = await _sut.Login(request, CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ShouldReturnUnauthorized()
    {
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "WrongPassword!"
        };

        _loginValidatorMock
            .Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _loginUseCaseMock
            .Setup(x => x.ExecuteAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<AuthResponse>("Email ou senha inválidos."));

        var result = await _sut.Login(request, CancellationToken.None);

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }
}