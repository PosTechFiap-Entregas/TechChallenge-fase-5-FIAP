using FiapX.Application.Extensions;
using FiapX.Application.Interfaces.UseCases;
using FiapX.Application.UseCases.Auth;
using FiapX.Application.UseCases.Videos;
using FiapX.Application.Validators;
using FluentAssertions;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace FiapX.Application.Tests.Extensions;

public class ApplicationServiceExtensionsTests
{
    [Fact]
    public void AddApplication_ShouldReturnSameServiceCollection()
    {
        var services = new ServiceCollection();

        var result = services.AddApplication();

        result.Should().BeSameAs(services);
    }

    [Fact]
    public void AddApplication_ShouldRegisterFluentValidators()
    {
        var services = new ServiceCollection();

        services.AddApplication();

        var hasValidator = services.Any(d =>
            d.ServiceType.IsGenericType &&
            d.ServiceType.GetGenericTypeDefinition() == typeof(IValidator<>));

        hasValidator.Should().BeTrue("AddValidatorsFromAssemblyContaining deveria registrar validators");
    }

    [Fact]
    public void AddApplication_ShouldRegisterValidatorsFromCorrectAssembly()
    {
        var services = new ServiceCollection();

        services.AddApplication();

        var hasRegisterUserValidator = services.Any(d =>
            d.ImplementationType == typeof(RegisterUserRequestValidator));

        hasRegisterUserValidator.Should().BeTrue(
            "RegisterUserRequestValidator deve estar registrado");
    }

    [Fact]
    public void AddApplication_ShouldRegisterRegisterUserUseCase()
    {
        var services = new ServiceCollection();

        services.AddApplication();

        var descriptor = services.FirstOrDefault(d =>
            d.ServiceType == typeof(IRegisterUserUseCase) &&
            d.ImplementationType == typeof(RegisterUserUseCase));

        descriptor.Should().NotBeNull();
    }

    [Fact]
    public void AddApplication_ShouldRegisterLoginUseCase()
    {
        var services = new ServiceCollection();

        services.AddApplication();

        var descriptor = services.FirstOrDefault(d =>
            d.ServiceType == typeof(ILoginUseCase) &&
            d.ImplementationType == typeof(LoginUseCase));

        descriptor.Should().NotBeNull();
    }

    [Fact]
    public void AddApplication_ShouldRegisterUploadVideoUseCase()
    {
        var services = new ServiceCollection();

        services.AddApplication();

        var descriptor = services.FirstOrDefault(d =>
            d.ServiceType == typeof(IUploadVideoUseCase) &&
            d.ImplementationType == typeof(UploadVideoUseCase));

        descriptor.Should().NotBeNull();
    }

    [Fact]
    public void AddApplication_ShouldRegisterGetUserVideosUseCase()
    {
        var services = new ServiceCollection();

        services.AddApplication();

        var descriptor = services.FirstOrDefault(d =>
            d.ServiceType == typeof(IGetUserVideosUseCase) &&
            d.ImplementationType == typeof(GetUserVideosUseCase));

        descriptor.Should().NotBeNull();
    }

    [Fact]
    public void AddApplication_ShouldRegisterGetVideoStatusUseCase()
    {
        var services = new ServiceCollection();

        services.AddApplication();

        var descriptor = services.FirstOrDefault(d =>
            d.ServiceType == typeof(IGetVideoStatusUseCase) &&
            d.ImplementationType == typeof(GetVideoStatusUseCase));

        descriptor.Should().NotBeNull();
    }

    [Fact]
    public void AddApplication_ShouldRegisterDownloadVideoUseCase()
    {
        var services = new ServiceCollection();

        services.AddApplication();

        var descriptor = services.FirstOrDefault(d =>
            d.ServiceType == typeof(IDownloadVideoUseCase) &&
            d.ImplementationType == typeof(DownloadVideoUseCase));

        descriptor.Should().NotBeNull();
    }

    [Fact]
    public void AddApplication_AllUseCases_ShouldBeRegisteredAsScoped()
    {
        var services = new ServiceCollection();

        services.AddApplication();

        var useCaseTypes = new[]
        {
            typeof(IRegisterUserUseCase),
            typeof(ILoginUseCase),
            typeof(IUploadVideoUseCase),
            typeof(IGetUserVideosUseCase),
            typeof(IGetVideoStatusUseCase),
            typeof(IDownloadVideoUseCase)
        };

        foreach (var useCaseType in useCaseTypes)
        {
            var descriptor = services.FirstOrDefault(d => d.ServiceType == useCaseType);
            descriptor.Should().NotBeNull($"{useCaseType.Name} deveria estar registrado");
            descriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped,
                $"{useCaseType.Name} deveria ser Scoped");
        }
    }
}