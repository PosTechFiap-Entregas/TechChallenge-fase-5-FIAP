using FiapX.Application.Interfaces;
using FiapX.Domain.Interfaces;
using FiapX.Infrastructure.Cache;
using FiapX.Infrastructure.Messaging;
using FiapX.Infrastructure.Persistence;
using FiapX.Infrastructure.Persistence.Context;
using FiapX.Infrastructure.Security;
using FiapX.Infrastructure.Services;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace FiapX.Infrastructure.Extensions;

/// <summary>
/// Extensões para configuração de dependências da camada de infraestrutura
/// </summary>
public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        return services.AddInfrastructure(configuration, includeMessaging: true);
    }

    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        bool includeMessaging)
    {
        // Database
        services.AddDatabase(configuration);

        // Repositories
        services.AddRepositories();

        // Cache
        services.AddRedisCache(configuration);

        // Messaging (opcional - Worker tem sua própria config)
        if (includeMessaging)
        {
            services.AddRabbitMqMessaging(configuration);
        }

        // Storage
        services.AddStorage(configuration);

        // JWT
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        return services;
    }

    private static IServiceCollection AddDatabase(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorCodesToAdd: null);
                npgsqlOptions.CommandTimeout(30);
            });

            // Apenas em desenvolvimento
            options.EnableSensitiveDataLogging(false);
            options.EnableDetailedErrors(false);
        });

        return services;
    }

    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        return services;
    }

    private static IServiceCollection AddRedisCache(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var redisConnection = configuration.GetConnectionString("Redis")
            ?? throw new InvalidOperationException("Redis connection string not found.");

        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var configurationOptions = ConfigurationOptions.Parse(redisConnection);
            configurationOptions.AbortOnConnectFail = false;
            configurationOptions.ConnectRetry = 3;
            configurationOptions.ConnectTimeout = 5000;
            return ConnectionMultiplexer.Connect(configurationOptions);
        });

        services.AddSingleton<ICacheService, RedisCacheService>();

        return services;
    }

    private static IServiceCollection AddRabbitMqMessaging(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMassTransit(busConfig =>
        {
            busConfig.UsingRabbitMq((context, cfg) =>
            {
                var host = configuration["RabbitMQ:Host"] ?? "localhost";
                var username = configuration["RabbitMQ:Username"] ?? "guest";
                var password = configuration["RabbitMQ:Password"] ?? "guest";

                cfg.Host(host, "/", h =>
                {
                    h.Username(username);
                    h.Password(password);
                });

                // Configurações de retry
                cfg.UseMessageRetry(r =>
                {
                    r.Incremental(3, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2));
                });

                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }

    private static IServiceCollection AddStorage(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<IStorageService, LocalStorageService>();
        services.AddScoped<IMessagePublisher, MassTransitMessagePublisher>();
        services.AddScoped<IVideoProcessingService, FFmpegVideoProcessingService>();
        return services;
    }
}