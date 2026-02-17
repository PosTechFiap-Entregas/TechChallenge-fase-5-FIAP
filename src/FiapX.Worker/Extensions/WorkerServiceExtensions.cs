using FiapX.Worker.Consumers;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FiapX.Worker.Extensions;

/// <summary>
/// Extensões de DI para o Worker
/// </summary>
public static class WorkerServiceExtensions
{
    public static IServiceCollection AddWorkerServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configurar MassTransit com Consumer
        services.AddMassTransit(busConfig =>
        {
            // Registrar o Consumer
            busConfig.AddConsumer<VideoUploadedEventConsumer>();

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

                // Configurar a fila para o Consumer
                cfg.ReceiveEndpoint("video-processing-queue", e =>
                {
                    e.ConfigureConsumer<VideoUploadedEventConsumer>(context);

                    // Retry policy
                    e.UseMessageRetry(r =>
                    {
                        r.Incremental(3, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2));
                    });

                    // Prefetch count (quantas mensagens pegar por vez)
                    e.PrefetchCount = 1;

                    // Concurrency limit (quantos workers processando ao mesmo tempo)
                    e.ConcurrentMessageLimit = 1;
                });
            });
        });

        return services;
    }
}