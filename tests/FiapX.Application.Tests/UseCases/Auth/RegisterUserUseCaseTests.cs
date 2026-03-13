using FiapX.Application.DTOs;
using FiapX.Application.Interfaces;
using FiapX.Application.UseCases.Auth;
using FiapX.Domain.Entities;
using FiapX.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace FiapX.Application.Tests.UseCases.Auth;

public class RegisterUserUseCaseTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IJwtTokenService> _jwtServiceMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly RegisterUserUseCase _sut;

    public RegisterUserUseCaseTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _jwtServiceMock = new Mock<IJwtTokenService>();
        _userRepositoryMock = new Mock<IUserRepository>();

        _unitOfWorkMock.Setup(x => x.Users).Returns(_userRepositoryMock.Object);

        _sut = new RegisterUserUseCase(_unitOfWorkMock.Object, _jwtServiceMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidData_ShouldCreateUserAndReturnToken()
    {
        var request = new RegisterUserRequest
        {
            Email = "newuser@test.com",
            Password = "Password123!",
            Name = "New User"
        };

        var expectedToken = "jwt-token-here";
        var expectedExpiration = DateTime.UtcNow.AddHours(1);

        _userRepositoryMock
            .Setup(x => x.EmailExistsAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _userRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _jwtServiceMock
            .Setup(x => x.GenerateToken(It.IsAny<User>()))
            .Returns(expectedToken);

        _jwtServiceMock
            .Setup(x => x.GetTokenExpiration())
            .Returns(expectedExpiration);

        var result = await _sut.ExecuteAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Email.Should().Be(request.Email);
        result.Value.Name.Should().Be(request.Name);
        result.Value.Token.Should().Be(expectedToken);
        result.Value.ExpiresAt.Should().Be(expectedExpiration);
        result.Value.UserId.Should().NotBe(Guid.Empty);

        _userRepositoryMock.Verify(
            x => x.EmailExistsAsync(request.Email, It.IsAny<CancellationToken>()),
            Times.Once);

        _userRepositoryMock.Verify(
            x => x.AddAsync(It.Is<User>(u =>
                u.Email == request.Email &&
                u.Name == request.Name &&
                u.IsActive), It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);

        _jwtServiceMock.Verify(
            x => x.GenerateToken(It.IsAny<User>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithExistingEmail_ShouldReturnFailure()
    {
        var request = new RegisterUserRequest
        {
            Email = "existing@test.com",
            Password = "Password123!",
            Name = "User"
        };

        _userRepositoryMock
            .Setup(x => x.EmailExistsAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _sut.ExecuteAsync(request);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Email já está sendo utilizado.");

        _userRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);

        _jwtServiceMock.Verify(
            x => x.GenerateToken(It.IsAny<User>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldHashPassword()
    {
        var request = new RegisterUserRequest
        {
            Email = "user@test.com",
            Password = "PlainTextPassword123!",
            Name = "User"
        };

        User? capturedUser = null;

        _userRepositoryMock
            .Setup(x => x.EmailExistsAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _userRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((user, _) => capturedUser = user)
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _jwtServiceMock
            .Setup(x => x.GenerateToken(It.IsAny<User>()))
            .Returns("token");

        _jwtServiceMock
            .Setup(x => x.GetTokenExpiration())
            .Returns(DateTime.UtcNow.AddHours(1));

        var result = await _sut.ExecuteAsync(request);

        result.IsSuccess.Should().BeTrue();
        capturedUser.Should().NotBeNull();
        capturedUser!.PasswordHash.Should().NotBe(request.Password);
        capturedUser.PasswordHash.Should().StartWith("$2");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCreateActiveUser()
    {
        var request = new RegisterUserRequest
        {
            Email = "user@test.com",
            Password = "Password123!",
            Name = "User"
        };

        User? capturedUser = null;

        _userRepositoryMock
            .Setup(x => x.EmailExistsAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _userRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((user, _) => capturedUser = user)
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _jwtServiceMock
            .Setup(x => x.GenerateToken(It.IsAny<User>()))
            .Returns("token");

        _jwtServiceMock
            .Setup(x => x.GetTokenExpiration())
            .Returns(DateTime.UtcNow.AddHours(1));

        await _sut.ExecuteAsync(request);

        capturedUser.Should().NotBeNull();
        capturedUser!.IsActive.Should().BeTrue();
    }

    [Theory]
    [InlineData("user@test.com", "Pass", "User")]
    [InlineData("user@test.com", "VeryLongPasswordThatExceedsSomeReasonableLimit", "User")]
    [InlineData("user@test.com", "Password123!", "A")]
    [InlineData("user@test.com", "Password123!", "Very Long Name That Might Test Some Boundary")]
    public async Task ExecuteAsync_WithVariousValidInputs_ShouldSucceed(
        string email, string password, string name)
    {
        var request = new RegisterUserRequest
        {
            Email = email,
            Password = password,
            Name = name
        };

        _userRepositoryMock
            .Setup(x => x.EmailExistsAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _userRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _jwtServiceMock
            .Setup(x => x.GenerateToken(It.IsAny<User>()))
            .Returns("token");

        _jwtServiceMock
            .Setup(x => x.GetTokenExpiration())
            .Returns(DateTime.UtcNow.AddHours(1));

        var result = await _sut.ExecuteAsync(request);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_WhenDatabaseFails_ShouldThrowException()
    {
        var request = new RegisterUserRequest
        {
            Email = "user@test.com",
            Password = "Password123!",
            Name = "User"
        };

        _userRepositoryMock
            .Setup(x => x.EmailExistsAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _userRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        await Assert.ThrowsAsync<Exception>(
            async () => await _sut.ExecuteAsync(request));

        _jwtServiceMock.Verify(
            x => x.GenerateToken(It.IsAny<User>()),
            Times.Never);
    }
}