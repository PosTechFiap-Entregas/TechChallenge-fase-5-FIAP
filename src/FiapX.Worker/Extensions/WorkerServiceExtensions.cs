using FiapX.Worker.Consumers;
using FiapX.Worker.Services;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FiapX.Worker.Extensions;

/// <summary>
/// Extensões de DI para o Worker com configurações de resiliência
/// </summary>
public static class WorkerServiceExtensions
{
    public static IServiceCollection AddWorkerServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMassTransit(busConfig =>
        {
            busConfig.AddConsumer<VideoUploadedEventConsumer>(typeof(VideoUploadedEventConsumerDefinition));

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

                cfg.ReceiveEndpoint("video-processing-queue", e =>
                {
                    e.ConfigureConsumer<VideoUploadedEventConsumer>(context);

                    e.UseMessageRetry(r =>
                    {
                        r.Exponential(
                            retryLimit: 3,
                            minInterval: TimeSpan.FromSeconds(1),
                            maxInterval: TimeSpan.FromSeconds(30),
                            intervalDelta: TimeSpan.FromSeconds(5)
                        );

                        r.Ignore<ArgumentNullException>();
                        r.Ignore<ArgumentException>();
                        r.Ignore<InvalidOperationException>();
                    });

                    e.UseCircuitBreaker(cb =>
                    {
                        cb.TrackingPeriod = TimeSpan.FromMinutes(1);  // Janela de tempo
                        cb.TripThreshold = 15;                        // 15% de taxa de erro
                        cb.ActiveThreshold = 10;                      // Mínimo 10 mensagens
                        cb.ResetInterval = TimeSpan.FromMinutes(5);   // Tenta reabrir após 5min
                    });

                    e.PrefetchCount = 16;
                });

                cfg.ReceiveEndpoint("video-processing-error-queue", e =>
                {
                    e.ConfigureConsumeTopology = false;

                    e.SetQueueArgument("x-message-ttl", 604800000);
                });

                cfg.ConfigureEndpoints(context);
            });
        });

        services.AddSingleton<VideoMetricsService>();

        return services;
    }
}

/// <summary>
/// Configuração avançada do Consumer com retry policy específico
/// </summary>
public class VideoUploadedEventConsumerDefinition : ConsumerDefinition<VideoUploadedEventConsumer>
{
    public VideoUploadedEventConsumerDefinition()
    {
        ConcurrentMessageLimit = 10;
    }

    protected override void ConfigureConsumer(
        IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<VideoUploadedEventConsumer> consumerConfigurator,
        IRegistrationContext context)
    {
        consumerConfigurator.UseMessageRetry(r =>
        {
            r.Exponential(
                retryLimit: 3,
                minInterval: TimeSpan.FromSeconds(1),
                maxInterval: TimeSpan.FromSeconds(30),
                intervalDelta: TimeSpan.FromSeconds(5)
            );

            r.Ignore<ArgumentNullException>();
            r.Ignore<ArgumentException>();
        });
    }
}