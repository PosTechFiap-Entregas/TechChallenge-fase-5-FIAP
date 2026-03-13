using Serilog;

namespace FiapX.API.Configurations;

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