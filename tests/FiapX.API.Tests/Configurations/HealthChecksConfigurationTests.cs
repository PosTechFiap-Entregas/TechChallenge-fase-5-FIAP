using FiapX.API.Configurations;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace FiapX.API.Tests.Configurations;

public class HealthChecksConfigurationTests
{
    private static IConfiguration BuildConfiguration(
        string postgresConnection = "Host=localhost;Database=fiapx;Username=fiapx;Password=fiapx",
        string redisConnection = "localhost:6379",
        string rabbitMqConnection = "amqp://fiapx:fiapx@localhost:5672")
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = postgresConnection,
                ["ConnectionStrings:Redis"] = redisConnection,
                ["HealthChecks:RabbitMQ"] = rabbitMqConnection
            })
            .Build();
    }

    private static ServiceCollection BuildServices()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        return services;
    }

    [Fact]
    public void AddHealthChecksConfiguration_ShouldReturnSameServiceCollection()
    {
        var services = BuildServices();
        var configuration = BuildConfiguration();

        var result = services.AddHealthChecksConfiguration(configuration);

        result.Should().BeSameAs(services);
    }

    [Fact]
    public void AddHealthChecksConfiguration_ShouldRegisterHealthCheckService()
    {
        var services = BuildServices();
        services.AddHealthChecksConfiguration(BuildConfiguration());
        var serviceProvider = services.BuildServiceProvider();

        var healthCheckService = serviceProvider.GetService<HealthCheckService>();

        healthCheckService.Should().NotBeNull();
    }

    [Fact]
    public void AddHealthChecksConfiguration_ShouldRegisterThreeHealthChecks()
    {
        var services = BuildServices();
        services.AddHealthChecksConfiguration(BuildConfiguration());
        var serviceProvider = services.BuildServiceProvider();

        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>().Value;

        options.Registrations.Should().HaveCount(3);
    }

    [Fact]
    public void AddHealthChecksConfiguration_ShouldRegisterPostgreSQLCheck()
    {
        var services = BuildServices();
        services.AddHealthChecksConfiguration(BuildConfiguration());
        var serviceProvider = services.BuildServiceProvider();

        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>().Value;

        options.Registrations.Should().Contain(r => r.Name == "PostgreSQL");
    }

    [Fact]
    public void AddHealthChecksConfiguration_ShouldRegisterRedisCheck()
    {
        var services = BuildServices();
        services.AddHealthChecksConfiguration(BuildConfiguration());
        var serviceProvider = services.BuildServiceProvider();

        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>().Value;

        options.Registrations.Should().Contain(r => r.Name == "Redis");
    }

    [Fact]
    public void AddHealthChecksConfiguration_ShouldRegisterRabbitMQCheck()
    {
        var services = BuildServices();
        services.AddHealthChecksConfiguration(BuildConfiguration());
        var serviceProvider = services.BuildServiceProvider();

        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>().Value;

        options.Registrations.Should().Contain(r => r.Name == "RabbitMQ");
    }

    [Fact]
    public void AddHealthChecksConfiguration_AllChecks_ShouldHaveExpectedNames()
    {
        var services = BuildServices();
        services.AddHealthChecksConfiguration(BuildConfiguration());
        var serviceProvider = services.BuildServiceProvider();

        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>().Value;
        var names = options.Registrations.Select(r => r.Name).ToList();

        names.Should().BeEquivalentTo(new[] { "PostgreSQL", "Redis", "RabbitMQ" });
    }

    [Fact]
    public void AddHealthChecksConfiguration_WhenCalled_ShouldNotThrow()
    {
        var services = BuildServices();
        var configuration = BuildConfiguration();

        var act = () => services.AddHealthChecksConfiguration(configuration);

        act.Should().NotThrow();
    }
}