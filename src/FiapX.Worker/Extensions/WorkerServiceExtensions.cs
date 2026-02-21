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
        // Configurar MassTransit com Consumer e Hardening
        services.AddMassTransit(busConfig =>
        {
            // Registrar o Consumer com Definition (para configurações avançadas)
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

                // ═══════════════════════════════════════════════════════════════════
                // CONFIGURAÇÃO DE RESILIÊNCIA
                // ═══════════════════════════════════════════════════════════════════

                // Configurar a fila principal para o Consumer
                cfg.ReceiveEndpoint("video-processing-queue", e =>
                {
                    e.ConfigureConsumer<VideoUploadedEventConsumer>(context);

                    // ────────────────────────────────────────────────────────────────
                    // RETRY POLICY - Backoff Exponencial
                    // ────────────────────────────────────────────────────────────────
                    e.UseMessageRetry(r =>
                    {
                        // Configuração: 3 tentativas com backoff exponencial
                        // Tentativa 1: delay 1s
                        // Tentativa 2: delay 5s
                        // Tentativa 3: delay 15s
                        r.Exponential(
                            retryLimit: 3,
                            minInterval: TimeSpan.FromSeconds(1),
                            maxInterval: TimeSpan.FromSeconds(30),
                            intervalDelta: TimeSpan.FromSeconds(5)
                        );

                        // Não fazer retry para erros de validação (são permanentes)
                        r.Ignore<ArgumentNullException>();
                        r.Ignore<ArgumentException>();
                        r.Ignore<InvalidOperationException>();
                    });

                    // ────────────────────────────────────────────────────────────────
                    // CIRCUIT BREAKER - Proteção contra sobrecarga
                    // ────────────────────────────────────────────────────────────────
                    e.UseCircuitBreaker(cb =>
                    {
                        cb.TrackingPeriod = TimeSpan.FromMinutes(1);  // Janela de tempo
                        cb.TripThreshold = 15;                        // 15% de taxa de erro
                        cb.ActiveThreshold = 10;                      // Mínimo 10 mensagens
                        cb.ResetInterval = TimeSpan.FromMinutes(5);   // Tenta reabrir após 5min
                    });

                    // ────────────────────────────────────────────────────────────────
                    // CONFIGURAÇÕES DE PERFORMANCE
                    // ────────────────────────────────────────────────────────────────
                    e.PrefetchCount = 16;  // Busca até 16 mensagens do RabbitMQ
                });

                // ════════════════════════════════════════════════════════════════════
                // DEAD LETTER QUEUE (DLQ)
                // ════════════════════════════════════════════════════════════════════
                // Mensagens que falharam após todas as tentativas vão para aqui
                cfg.ReceiveEndpoint("video-processing-error-queue", e =>
                {
                    // Esta fila só armazena, não processa
                    e.ConfigureConsumeTopology = false;

                    // Manter mensagens por 7 dias
                    e.SetQueueArgument("x-message-ttl", 604800000); // 7 dias em ms
                });

                cfg.ConfigureEndpoints(context);
            });
        });

        // Métricas Prometheus
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
        // Limitar concorrência por consumer instance
        ConcurrentMessageLimit = 10;
    }

    protected override void ConfigureConsumer(
        IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<VideoUploadedEventConsumer> consumerConfigurator,
        IRegistrationContext context)
    {
        // Retry policy específico do consumer (sobrescreve o global se necessário)
        consumerConfigurator.UseMessageRetry(r =>
        {
            r.Exponential(
                retryLimit: 3,
                minInterval: TimeSpan.FromSeconds(1),
                maxInterval: TimeSpan.FromSeconds(30),
                intervalDelta: TimeSpan.FromSeconds(5)
            );

            // Ignorar erros de validação
            r.Ignore<ArgumentNullException>();
            r.Ignore<ArgumentException>();
        });
    }
}