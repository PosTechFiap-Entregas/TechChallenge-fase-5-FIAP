using FiapX.Application.Extensions;
using FiapX.Infrastructure.Extensions;
using FiapX.Worker.Extensions;
using Prometheus; // ← ADICIONADO
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

try
{
    Log.Information("Iniciando FiapX Worker...");

    var builder = Host.CreateDefaultBuilder(args)
        .UseSerilog((context, services, configuration) => configuration
            .ReadFrom.Configuration(context.Configuration)
            .Enrich.FromLogContext())
        .ConfigureServices((hostContext, services) =>
        {
            services.AddApplication();

            services.AddInfrastructure(hostContext.Configuration, includeMessaging: false);

            services.AddWorkerServices(hostContext.Configuration);
        });

    var host = builder.Build();

    // ═══════════════════════════════════════════════════════════════════════
    // PROMETHEUS METRICS SERVER
    // ═══════════════════════════════════════════════════════════════════════
    // Inicia servidor HTTP na porta 9090 para expor métricas
    var metricServer = new KestrelMetricServer(port: 9091);
    metricServer.Start();
    Log.Information("Prometheus Metrics Server rodando em http://localhost:9091/metrics");
    // ═══════════════════════════════════════════════════════════════════════

    Log.Information("FiapX Worker iniciado e escutando a fila RabbitMQ...");

    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Worker falhou ao iniciar");
    throw;
}
finally
{
    await Log.CloseAndFlushAsync();
}