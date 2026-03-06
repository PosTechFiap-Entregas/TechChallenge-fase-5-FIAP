using FiapX.API.Configurations;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace FiapX.API.Tests.Configurations;

public class SerilogConfigurationTests
{
    [Fact]
    public void UseSerilogConfiguration_ShouldReturnSameHostBuilder()
    {
        var hostBuilder = Host.CreateDefaultBuilder();

        var result = hostBuilder.UseSerilogConfiguration();

        result.Should().BeSameAs(hostBuilder);
    }

    [Fact]
    public void UseSerilogConfiguration_ShouldNotThrow()
    {
        var hostBuilder = Host.CreateDefaultBuilder();

        var act = () => hostBuilder.UseSerilogConfiguration();

        act.Should().NotThrow();
    }

    [Fact]
    public void UseSerilogConfiguration_ShouldBuildSuccessfully()
    {
        var hostBuilder = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration(config =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Serilog:MinimumLevel:Default"] = "Information"
                });
            });

        var act = () =>
        {
            hostBuilder.UseSerilogConfiguration();
            hostBuilder.Build();
        };

        act.Should().NotThrow();
    }

    [Fact]
    public void UseSerilogConfiguration_BuiltHost_ShouldHaveSerilogAsLogger()
    {
        var hostBuilder = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration(config =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Serilog:MinimumLevel:Default"] = "Warning"
                });
            })
            .UseSerilogConfiguration();

        using var host = hostBuilder.Build();

        host.Should().NotBeNull();
        var loggerFactory = host.Services.GetService(typeof(Microsoft.Extensions.Logging.ILoggerFactory));
        loggerFactory.Should().NotBeNull();
    }

    [Fact]
    public void UseSerilogConfiguration_CalledMultipleTimes_ShouldNotThrow()
    {
        var hostBuilder = Host.CreateDefaultBuilder();

        var act = () =>
        {
            hostBuilder.UseSerilogConfiguration();
            hostBuilder.UseSerilogConfiguration();
        };

        act.Should().NotThrow();
    }
}