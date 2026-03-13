using FiapX.Application.DTOs;
using FiapX.Application.Interfaces;
using FiapX.Application.UseCases.Auth;
using FiapX.Domain.Entities;
using FiapX.Domain.Interfaces;
using FiapX.Shared.Security;
using FluentAssertions;
using Moq;

namespace FiapX.Application.Tests.UseCases.Auth;

public class LoginUseCaseTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IJwtTokenService> _jwtServiceMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly LoginUseCase _sut;

    public LoginUseCaseTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _jwtServiceMock = new Mock<IJwtTokenService>();
        _userRepositoryMock = new Mock<IUserRepository>();

        _unitOfWorkMock.Setup(x => x.Users).Returns(_userRepositoryMock.Object);

        _sut = new LoginUseCase(_unitOfWorkMock.Object, _jwtServiceMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidCredentials_ShouldReturnSuccessWithToken()
    {
        var email = "user@test.com";
        var password = "Password123!";
        var passwordHash = PasswordHasher.HashPassword(password);

        var user = new User(email, passwordHash, "Test User");

        var request = new LoginRequest
        {
            Email = email,
            Password = password
        };

        var expectedToken = "jwt-token-here";
        var expectedExpiration = DateTime.UtcNow.AddHours(1);

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _jwtServiceMock
            .Setup(x => x.GenerateToken(user))
            .Returns(expectedToken);

        _jwtServiceMock
            .Setup(x => x.GetTokenExpiration())
            .Returns(expectedExpiration);

        var result = await _sut.ExecuteAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.UserId.Should().Be(user.Id);
        result.Value.Email.Should().Be(email);
        result.Value.Name.Should().Be("Test User");
        result.Value.Token.Should().Be(expectedToken);
        result.Value.ExpiresAt.Should().Be(expectedExpiration);

        _userRepositoryMock.Verify(
            x => x.GetByEmailAsync(email, It.IsAny<CancellationToken>()),
            Times.Once);

        _jwtServiceMock.Verify(
            x => x.GenerateToken(user),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithNonExistentEmail_ShouldReturnFailure()
    {
        var request = new LoginRequest
        {
            Email = "nonexistent@test.com",
            Password = "Password123!"
        };

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await _sut.ExecuteAsync(request);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Email ou senha inválidos.");

        _jwtServiceMock.Verify(
            x => x.GenerateToken(It.IsAny<User>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidPassword_ShouldReturnFailure()
    {
        var email = "user@test.com";
        var correctPassword = "CorrectPassword123!";
        var wrongPassword = "WrongPassword123!";
        var passwordHash = PasswordHasher.HashPassword(correctPassword);

        var user = new User(email, passwordHash, "Test User");

        var request = new LoginRequest
        {
            Email = email,
            Password = wrongPassword
        };

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _sut.ExecuteAsync(request);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Email ou senha inválidos.");

        _jwtServiceMock.Verify(
            x => x.GenerateToken(It.IsAny<User>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WithInactiveUser_ShouldReturnFailure()
    {
        var email = "user@test.com";
        var password = "Password123!";
        var passwordHash = PasswordHasher.HashPassword(password);

        var user = new User(email, passwordHash, "Test User");
        user.Deactivate();

        var request = new LoginRequest
        {
            Email = email,
            Password = password
        };

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _sut.ExecuteAsync(request);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Conta desativada.");

        _jwtServiceMock.Verify(
            x => x.GenerateToken(It.IsAny<User>()),
            Times.Never);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task ExecuteAsync_WithEmptyEmail_ShouldHandleGracefully(string email)
    {
        var request = new LoginRequest
        {
            Email = email,
            Password = "Password123!"
        };

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await _sut.ExecuteAsync(request);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Email ou senha inválidos.");
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancelled_ShouldRespectCancellationToken()
    {
        var request = new LoginRequest
        {
            Email = "user@test.com",
            Password = "Password123!"
        };

        var cts = new CancellationTokenSource();
        cts.Cancel();

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        await Assert.ThrowsAsync<OperationCanceledException>(
            async () => await _sut.ExecuteAsync(request, cts.Token));
    }
}