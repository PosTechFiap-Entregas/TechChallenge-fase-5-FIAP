using FiapX.Application.Extensions;
using FiapX.Infrastructure.Extensions;
using FiapX.Worker.Extensions;
using Serilog;

// ─── Serilog ────────────────────────────────────────────────────────────────
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

try
{
    Log.Information("Iniciando FiapX Worker...");

    // ─── Builder ────────────────────────────────────────────────────────────
    var builder = Host.CreateDefaultBuilder(args)
        .UseSerilog((context, services, configuration) => configuration
            .ReadFrom.Configuration(context.Configuration)
            .Enrich.FromLogContext())
        .ConfigureServices((hostContext, services) =>
        {
            // Application
            services.AddApplication();

            // Infrastructure SEM RabbitMQ (Worker tem sua própria config)
            services.AddInfrastructure(hostContext.Configuration, includeMessaging: false);

            // Worker (MassTransit com Consumer)
            services.AddWorkerServices(hostContext.Configuration);
        });

    // ─── Host ───────────────────────────────────────────────────────────────
    var host = builder.Build();

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