using FiapX.Application.Interfaces;
using FiapX.Domain.Interfaces;
using FiapX.Infrastructure.Cache;
using FiapX.Infrastructure.Extensions;
using FiapX.Infrastructure.Messaging;
using FiapX.Infrastructure.Persistence;
using FiapX.Infrastructure.Persistence.Context;
using FiapX.Infrastructure.Security;
using FiapX.Infrastructure.Services;
using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace FiapX.Infrastructure.Tests.Extensions;

public class InfrastructureServiceExtensionsTests
{
    private static IConfiguration BuildConfiguration(
    string? postgresConnection = "Host=localhost;Database=test;Username=test;Password=test",
    string? redisConnection = "localhost:6379",
    string? rabbitHost = "localhost",
    string? rabbitUser = "guest",
    string? rabbitPass = "guest",
    string? jwtSecret = "super-secret-key-for-testing-purposes-only-32chars",
    string? jwtIssuer = "test-issuer",
    string? jwtAudience = "test-audience",
    string? jwtExpirationMinutes = "60")
    {
        var settings = new Dictionary<string, string?>();

        if (postgresConnection != null)
            settings["ConnectionStrings:DefaultConnection"] = postgresConnection;

        if (redisConnection != null)
            settings["ConnectionStrings:Redis"] = redisConnection;

        if (rabbitHost != null)
            settings["RabbitMQ:Host"] = rabbitHost;

        if (rabbitUser != null)
            settings["RabbitMQ:Username"] = rabbitUser;

        if (rabbitPass != null)
            settings["RabbitMQ:Password"] = rabbitPass;

        if (jwtSecret != null)
            settings["JWT:Secret"] = jwtSecret;

        if (jwtIssuer != null)
            settings["JWT:Issuer"] = jwtIssuer;

        if (jwtAudience != null)
            settings["JWT:Audience"] = jwtAudience;

        if (jwtExpirationMinutes != null)
            settings["JWT:ExpirationMinutes"] = jwtExpirationMinutes;

        return new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();
    }

    private static ServiceCollection BuildServices(IConfiguration configuration)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(configuration);
        return services;
    }

    [Fact]
    public void AddInfrastructure_ShouldReturnSameServiceCollection()
    {
        var services = new ServiceCollection();
        var configuration = BuildConfiguration();

        var result = services.AddInfrastructure(configuration);

        result.Should().BeSameAs(services);
    }

    [Fact]
    public void AddInfrastructure_ShouldRegisterConnectionMultiplexerAsSingleton()
    {
        var services = new ServiceCollection();
        var configuration = BuildConfiguration();

        services.AddInfrastructure(configuration);

        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IConnectionMultiplexer));
        descriptor.Should().NotBeNull();
        descriptor!.Lifetime.Should().Be(ServiceLifetime.Singleton);
    }

    [Fact]
    public void AddInfrastructure_WithIncludeMessagingFalse_ShouldNotRegisterMassTransit()
    {
        var services = new ServiceCollection();
        var configuration = BuildConfiguration();

        services.AddInfrastructure(configuration, includeMessaging: false);

        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IBus));
        descriptor.Should().BeNull();
    }

    [Fact]
    public void AddInfrastructure_WithIncludeMessagingTrue_ShouldRegisterMassTransit()
    {
        var services = new ServiceCollection();
        var configuration = BuildConfiguration();

        services.AddInfrastructure(configuration, includeMessaging: true);

        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IBus));
        descriptor.Should().NotBeNull();
    }

    [Fact]
    public void AddInfrastructure_DefaultOverload_ShouldIncludeMessaging()
    {
        var services = new ServiceCollection();
        var configuration = BuildConfiguration();

        services.AddInfrastructure(configuration);

        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IBus));
        descriptor.Should().NotBeNull();
    }

    [Fact]
    public void AddInfrastructure_ShouldRegisterStorageServiceDescriptor()
    {
        var services = new ServiceCollection();
        var configuration = BuildConfiguration();

        services.AddInfrastructure(configuration);

        var descriptor = services.FirstOrDefault(d =>
            d.ServiceType == typeof(IStorageService) &&
            d.ImplementationType == typeof(LocalStorageService));

        descriptor.Should().NotBeNull();
    }

    [Fact]
    public void AddInfrastructure_ShouldRegisterMessagePublisherDescriptor()
    {
        var services = new ServiceCollection();
        var configuration = BuildConfiguration();

        services.AddInfrastructure(configuration);

        var descriptor = services.FirstOrDefault(d =>
            d.ServiceType == typeof(IMessagePublisher) &&
            d.ImplementationType == typeof(MassTransitMessagePublisher));

        descriptor.Should().NotBeNull();
    }

    [Fact]
    public void AddInfrastructure_ShouldRegisterVideoProcessingServiceDescriptor()
    {
        var services = new ServiceCollection();
        var configuration = BuildConfiguration();

        services.AddInfrastructure(configuration);

        var descriptor = services.FirstOrDefault(d =>
            d.ServiceType == typeof(IVideoProcessingService) &&
            d.ImplementationType == typeof(FFmpegVideoProcessingService));

        descriptor.Should().NotBeNull();
    }

    [Fact]
    public void AddInfrastructure_ShouldRegisterTelegramNotificationServiceDescriptor()
    {
        var services = new ServiceCollection();
        var configuration = BuildConfiguration();

        services.AddInfrastructure(configuration);

        var descriptor = services.FirstOrDefault(d =>
            d.ServiceType == typeof(ITelegramNotificationService) &&
            d.ImplementationType == typeof(TelegramNotificationService));

        descriptor.Should().NotBeNull();
    }

    [Fact]
    public void AddInfrastructure_ShouldRegisterJwtTokenServiceDescriptor()
    {
        var services = new ServiceCollection();
        var configuration = BuildConfiguration();

        services.AddInfrastructure(configuration);

        var descriptor = services.FirstOrDefault(d =>
            d.ServiceType == typeof(IJwtTokenService) &&
            d.ImplementationType == typeof(JwtTokenService));

        descriptor.Should().NotBeNull();
    }

    [Fact]
    public void AddInfrastructure_ShouldRegisterUnitOfWorkDescriptor()
    {
        var services = new ServiceCollection();
        var configuration = BuildConfiguration();

        services.AddInfrastructure(configuration);

        var descriptor = services.FirstOrDefault(d =>
            d.ServiceType == typeof(IUnitOfWork) &&
            d.ImplementationType == typeof(UnitOfWork));

        descriptor.Should().NotBeNull();
    }

    [Fact]
    public void AddInfrastructure_ShouldRegisterRedisCacheServiceDescriptor()
    {
        var services = new ServiceCollection();
        var configuration = BuildConfiguration();

        services.AddInfrastructure(configuration);

        var descriptor = services.FirstOrDefault(d =>
            d.ServiceType == typeof(ICacheService) &&
            d.ImplementationType == typeof(RedisCacheService));

        descriptor.Should().NotBeNull();
    }

    [Fact]
    public void AddInfrastructure_ShouldRegisterDbContext()
    {
        var configuration = BuildConfiguration();
        var services = BuildServices(configuration);

        services.AddInfrastructure(configuration);
        var serviceProvider = services.BuildServiceProvider();

        var dbContext = serviceProvider.GetService<AppDbContext>();
        dbContext.Should().NotBeNull();
    }

    [Fact]
    public void AddInfrastructure_ShouldRegisterUnitOfWork()
    {
        var configuration = BuildConfiguration();
        var services = BuildServices(configuration);

        services.AddInfrastructure(configuration);
        var serviceProvider = services.BuildServiceProvider();

        var unitOfWork = serviceProvider.GetService<IUnitOfWork>();
        unitOfWork.Should().NotBeNull();
    }

    [Fact]
    public void AddInfrastructure_ShouldRegisterRedisCacheService()
    {
        var configuration = BuildConfiguration();
        var services = BuildServices(configuration);

        services.AddInfrastructure(configuration);

        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(ICacheService));
        descriptor.Should().NotBeNull();
        descriptor!.ImplementationType.Should().Be(typeof(RedisCacheService));
    }

    [Fact]
    public void AddInfrastructure_ShouldRegisterStorageService()
    {
        var configuration = BuildConfiguration();
        var services = BuildServices(configuration);

        services.AddInfrastructure(configuration);
        var serviceProvider = services.BuildServiceProvider();

        var storageService = serviceProvider.GetService<IStorageService>();
        storageService.Should().NotBeNull();
        storageService.Should().BeOfType<LocalStorageService>();
    }

    [Fact]
    public void AddInfrastructure_ShouldRegisterTelegramNotificationService()
    {
        var configuration = BuildConfiguration();
        var services = BuildServices(configuration);

        services.AddInfrastructure(configuration);
        var serviceProvider = services.BuildServiceProvider();

        var telegramService = serviceProvider.GetService<ITelegramNotificationService>();
        telegramService.Should().NotBeNull();
        telegramService.Should().BeOfType<TelegramNotificationService>();
    }

    [Fact]
    public void AddInfrastructure_ShouldRegisterJwtTokenService()
    {
        var configuration = BuildConfiguration();
        var services = BuildServices(configuration);

        services.AddInfrastructure(configuration);
        var serviceProvider = services.BuildServiceProvider();

        var jwtService = serviceProvider.GetService<IJwtTokenService>();
        jwtService.Should().NotBeNull();
        jwtService.Should().BeOfType<JwtTokenService>();
    }

    [Fact]
    public void AddInfrastructure_WithoutPostgresConnection_ShouldThrowInvalidOperationException()
    {
        var services = new ServiceCollection();
        var configuration = BuildConfiguration(postgresConnection: null);

        var act = () => services.AddInfrastructure(configuration);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*DefaultConnection*");
    }

    [Fact]
    public void AddInfrastructure_WithoutRedisConnection_ShouldThrowInvalidOperationException()
    {
        var services = new ServiceCollection();
        var configuration = BuildConfiguration(redisConnection: null);

        var act = () => services.AddInfrastructure(configuration);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Redis*");
    }
}