using HealthChecks.NpgSql;
using HealthChecks.Redis;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace FiapX.API.Configurations;

public static class HealthChecksConfiguration
{
    public static IServiceCollection AddHealthChecksConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var healthChecksBuilder = services.AddHealthChecks();

        healthChecksBuilder.AddNpgSql(
            configuration.GetConnectionString("DefaultConnection") ?? "",
            name: "PostgreSQL");

        healthChecksBuilder.AddRedis(
            configuration.GetConnectionString("Redis") ?? "",
            name: "Redis");

        healthChecksBuilder.AddRabbitMQ(
            configuration["HealthChecks:RabbitMQ"] ?? "",
            name: "RabbitMQ");

        return services;
    }

    public static WebApplication UseHealthChecksConfiguration(this WebApplication app)
    {
        app.MapHealthChecks("/health");

        app.MapHealthChecks("/health/ready");

        return app;
    }
}