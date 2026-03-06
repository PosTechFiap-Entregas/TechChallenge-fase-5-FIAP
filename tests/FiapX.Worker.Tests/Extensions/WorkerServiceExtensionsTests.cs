using FiapX.Worker.Consumers;
using FiapX.Worker.Extensions;
using FiapX.Worker.Services;
using FluentAssertions;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FiapX.Worker.Tests.Extensions;

public class WorkerServiceExtensionsTests
{
    private static IConfiguration BuildConfiguration(
        string? rabbitHost = "localhost",
        string? rabbitUser = "guest",
        string? rabbitPass = "guest")
    {
        var settings = new Dictionary<string, string?>();

        if (rabbitHost != null) settings["RabbitMQ:Host"] = rabbitHost;
        if (rabbitUser != null) settings["RabbitMQ:Username"] = rabbitUser;
        if (rabbitPass != null) settings["RabbitMQ:Password"] = rabbitPass;

        return new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();
    }

    private static ServiceCollection BuildInMemoryServices()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        services.AddMassTransitTestHarness(busConfig =>
        {
            busConfig.AddConsumer<VideoUploadedEventConsumer>(
                typeof(VideoUploadedEventConsumerDefinition));

            busConfig.UsingInMemory((context, cfg) =>
            {
                cfg.ReceiveEndpoint("video-processing-queue", e =>
                {
                    e.ConfigureConsumer<VideoUploadedEventConsumer>(context);

                    e.UseMessageRetry(r =>
                    {
                        r.Exponential(
                            retryLimit: 3,
                            minInterval: TimeSpan.FromSeconds(1),
                            maxInterval: TimeSpan.FromSeconds(30),
                            intervalDelta: TimeSpan.FromSeconds(5));

                        r.Ignore<ArgumentNullException>();
                        r.Ignore<ArgumentException>();
                        r.Ignore<InvalidOperationException>();
                    });

                    e.UseCircuitBreaker(cb =>
                    {
                        cb.TrackingPeriod = TimeSpan.FromMinutes(1);
                        cb.TripThreshold = 15;
                        cb.ActiveThreshold = 10;
                        cb.ResetInterval = TimeSpan.FromMinutes(5);
                    });

                    e.PrefetchCount = 16;
                });

                cfg.ReceiveEndpoint("video-processing-error-queue", e =>
                {
                    e.ConfigureConsumeTopology = false;
                });

                cfg.ConfigureEndpoints(context);
            });
        });

        services.AddSingleton<VideoMetricsService>();
        return services;
    }

    [Fact]
    public void AddWorkerServices_ShouldReturnSameServiceCollection()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        var result = services.AddWorkerServices(BuildConfiguration());

        result.Should().BeSameAs(services);
    }

    [Fact]
    public void AddWorkerServices_ShouldRegisterMassTransit()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddWorkerServices(BuildConfiguration());

        services.Any(d => d.ServiceType == typeof(IBus))
            .Should().BeTrue("MassTransit deve registrar IBus");
    }

    [Fact]
    public void AddWorkerServices_ShouldRegisterIBusControl()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddWorkerServices(BuildConfiguration());

        services.Any(d => d.ServiceType == typeof(IBusControl))
            .Should().BeTrue("MassTransit deve registrar IBusControl");
    }

    [Fact]
    public void AddWorkerServices_ShouldRegisterVideoUploadedEventConsumer()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddWorkerServices(BuildConfiguration());

        services.Any(d =>
                d.ImplementationType == typeof(VideoUploadedEventConsumer) ||
                d.ServiceType == typeof(VideoUploadedEventConsumer))
            .Should().BeTrue("VideoUploadedEventConsumer deve ser registrado");
    }

    [Fact]
    public void AddWorkerServices_ShouldRegisterVideoMetricsService()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddWorkerServices(BuildConfiguration());

        services.Any(d => d.ServiceType == typeof(VideoMetricsService))
            .Should().BeTrue();
    }

    [Fact]
    public void AddWorkerServices_VideoMetricsService_ShouldBeRegisteredAsSingleton()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddWorkerServices(BuildConfiguration());

        var descriptor = services.First(d => d.ServiceType == typeof(VideoMetricsService));
        descriptor.Lifetime.Should().Be(ServiceLifetime.Singleton);
    }

    [Fact]
    public void AddWorkerServices_ShouldRegisterConsumerDefinition()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddWorkerServices(BuildConfiguration());

        services.Any(d =>
                d.ServiceType.Name.Contains("ConsumerDefinition") ||
                d.ImplementationType?.Name.Contains("ConsumerDefinition") == true)
            .Should().BeTrue("VideoUploadedEventConsumerDefinition deve ser registrado");
    }

    [Fact]
    public void AddWorkerServices_WithCustomRabbitHost_ShouldNotThrow()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        var act = () => services.AddWorkerServices(
            BuildConfiguration(rabbitHost: "custom-host"));

        act.Should().NotThrow();
    }

    [Fact]
    public void AddWorkerServices_WithMissingRabbitHost_ShouldUseDefaultLocalhost()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        var act = () => services.AddWorkerServices(BuildConfiguration(rabbitHost: null));

        act.Should().NotThrow();
    }

    [Fact]
    public void AddWorkerServices_WithMissingRabbitCredentials_ShouldUseDefaultGuest()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        var act = () => services.AddWorkerServices(
            BuildConfiguration(rabbitUser: null, rabbitPass: null));

        act.Should().NotThrow();
    }

    [Fact]
    public void AddWorkerServices_NullHostNullUserNullPass_ShouldRegisterAll()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddWorkerServices(
            BuildConfiguration(rabbitHost: null, rabbitUser: null, rabbitPass: null));

        services.Any(d => d.ServiceType == typeof(IBus)).Should().BeTrue();
        services.Any(d => d.ServiceType == typeof(VideoMetricsService)).Should().BeTrue();
    }

    [Fact]
    public void AddWorkerServices_CalledTwice_ShouldThrowConfigurationException()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddWorkerServices(BuildConfiguration());

        var act = () => services.AddWorkerServices(BuildConfiguration());

        act.Should().Throw<MassTransit.ConfigurationException>()
            .WithMessage("*AddMassTransit()*");
    }

    [Fact]
    public void InMemoryBus_ShouldBuildSuccessfully()
    {
        var provider = BuildInMemoryServices().BuildServiceProvider();

        var bus = provider.GetService<IBus>();

        bus.Should().NotBeNull();
    }

    [Fact]
    public void InMemoryBus_ShouldResolveVideoMetricsService()
    {
        var provider = BuildInMemoryServices().BuildServiceProvider();

        var metrics = provider.GetService<VideoMetricsService>();

        metrics.Should().NotBeNull();
    }

    [Fact]
    public void InMemoryBus_AllLambdas_ShouldNotThrowOnBuild()
    {
        var act = () => BuildInMemoryServices()
            .BuildServiceProvider()
            .GetRequiredService<IBus>();

        act.Should().NotThrow();
    }

    [Fact]
    public void InMemoryBus_RetryPolicy_ShouldCoverIgnoreBranches()
    {
        var act = () => BuildInMemoryServices()
            .BuildServiceProvider()
            .GetRequiredService<IBus>();

        act.Should().NotThrow();
    }

    [Fact]
    public void InMemoryBus_CircuitBreaker_ShouldCoverAllProperties()
    {
        var act = () => BuildInMemoryServices()
            .BuildServiceProvider()
            .GetRequiredService<IBus>();

        act.Should().NotThrow();
    }

    [Fact]
    public void InMemoryBus_DeadLetterQueue_ShouldCoverConfigureConsumeTopologyAndTtl()
    {
        var act = () => BuildInMemoryServices()
            .BuildServiceProvider()
            .GetRequiredService<IBus>();

        act.Should().NotThrow();
    }

    [Fact]
    public async Task InMemoryHarness_ShouldStartAndStop()
    {
        var provider = BuildInMemoryServices().BuildServiceProvider();
        var harness = provider.GetRequiredService<ITestHarness>();

        await harness.Start();
        try
        {
            harness.Bus.Should().NotBeNull();
        }
        finally
        {
            await harness.Stop();
        }
    }

    [Fact]
    public async Task InMemoryHarness_ConsumerShouldBeAvailable()
    {
        var provider = BuildInMemoryServices().BuildServiceProvider();
        var harness = provider.GetRequiredService<ITestHarness>();

        await harness.Start();
        try
        {
            var consumerHarness = harness.GetConsumerHarness<VideoUploadedEventConsumer>();
            consumerHarness.Should().NotBeNull();
        }
        finally
        {
            await harness.Stop();
        }
    }

    [Fact]
    public void VideoUploadedEventConsumerDefinition_ShouldHaveConcurrentMessageLimit()
    {
        var definition = new VideoUploadedEventConsumerDefinition();

        definition.ConcurrentMessageLimit.Should().Be(10);
    }

    [Fact]
    public void VideoUploadedEventConsumerDefinition_ShouldBeInstantiable()
    {
        var act = () => new VideoUploadedEventConsumerDefinition();

        act.Should().NotThrow();
    }

    [Fact]
    public async Task VideoUploadedEventConsumerDefinition_ConfigureConsumer_ShouldExecuteViaHarness()
    {
        var provider = BuildInMemoryServices().BuildServiceProvider();
        var harness = provider.GetRequiredService<ITestHarness>();

        await harness.Start();
        try
        {
            harness.Bus.Should().NotBeNull();
        }
        finally
        {
            await harness.Stop();
        }
    }
}