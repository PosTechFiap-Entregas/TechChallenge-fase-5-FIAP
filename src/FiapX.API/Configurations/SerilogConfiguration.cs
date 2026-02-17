using Serilog;

namespace FiapX.API.Configurations;

/// <summary>
/// Configurações do Serilog para logging estruturado
/// </summary>
public static class SerilogConfiguration
{
    public static IHostBuilder UseSerilogConfiguration(this IHostBuilder builder)
    {
        builder.UseSerilog((context, services, loggerConfiguration) =>
        {
            loggerConfiguration
                .ReadFrom.Configuration(context.Configuration)
                .Enrich.FromLogContext();
        });

        return builder;
    }
}