using HealthChecks.NpgSql;
using HealthChecks.Redis;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace FiapX.API.Configurations;

/// <summary>
/// Configurações de Health Checks para monitoramento dos serviços
/// </summary>
public static class HealthChecksConfiguration
{
    public static IServiceCollection AddHealthChecksConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var healthChecksBuilder = services.AddHealthChecks();

        // PostgreSQL
        healthChecksBuilder.AddNpgSql(
            configuration.GetConnectionString("DefaultConnection") ?? "",
            name: "PostgreSQL");

        // Redis
        healthChecksBuilder.AddRedis(
            configuration.GetConnectionString("Redis") ?? "",
            name: "Redis");

        // RabbitMQ
        healthChecksBuilder.AddRabbitMQ(
            configuration["HealthChecks:RabbitMQ"] ?? "",
            name: "RabbitMQ");

        return services;
    }

    public static WebApplication UseHealthChecksConfiguration(this WebApplication app)
    {
        // Health check geral - retorna status de todos os serviços
        app.MapHealthChecks("/health");

        // Health check de prontidão - verifica todas as dependências
        app.MapHealthChecks("/health/ready");

        return app;
    }
}