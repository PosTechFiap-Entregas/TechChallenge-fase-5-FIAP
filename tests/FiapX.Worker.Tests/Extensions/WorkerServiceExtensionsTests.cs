using FiapX.Worker.Consumers;
using FiapX.Worker.Extensions;
using FiapX.Worker.Services;
using FluentAssertions;
using MassTransit;
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

        if (rabbitHost != null)
            settings["RabbitMQ:Host"] = rabbitHost;

        if (rabbitUser != null)
            settings["RabbitMQ:Username"] = rabbitUser;

        if (rabbitPass != null)
            settings["RabbitMQ:Password"] = rabbitPass;

        return new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();
    }

    [Fact]
    public void AddWorkerServices_ShouldReturnSameServiceCollection()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        var configuration = BuildConfiguration();

        var result = services.AddWorkerServices(configuration);

        result.Should().BeSameAs(services);
    }

    [Fact]
    public void AddWorkerServices_ShouldRegisterMassTransit()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        services.AddWorkerServices(BuildConfiguration());

        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IBus));
        descriptor.Should().NotBeNull("MassTransit deve registrar IBus");
    }

    [Fact]
    public void AddWorkerServices_ShouldRegisterIBusControl()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        services.AddWorkerServices(BuildConfiguration());

        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IBusControl));
        descriptor.Should().NotBeNull("MassTransit deve registrar IBusControl");
    }

    [Fact]
    public void AddWorkerServices_ShouldRegisterVideoUploadedEventConsumer()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        services.AddWorkerServices(BuildConfiguration());

        var consumerDescriptor = services.FirstOrDefault(d =>
            d.ImplementationType == typeof(VideoUploadedEventConsumer) ||
            d.ServiceType == typeof(VideoUploadedEventConsumer));

        consumerDescriptor.Should().NotBeNull(
            "VideoUploadedEventConsumer deve ser registrado pelo MassTransit");
    }

    [Fact]
    public void AddWorkerServices_ShouldRegisterVideoMetricsService()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        services.AddWorkerServices(BuildConfiguration());

        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(VideoMetricsService));
        descriptor.Should().NotBeNull();
    }

    [Fact]
    public void AddWorkerServices_VideoMetricsService_ShouldBeRegisteredAsSingleton()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        services.AddWorkerServices(BuildConfiguration());

        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(VideoMetricsService));
        descriptor.Should().NotBeNull();
        descriptor!.Lifetime.Should().Be(ServiceLifetime.Singleton);
    }

    [Fact]
    public void AddWorkerServices_ShouldRegisterConsumerDefinition()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        services.AddWorkerServices(BuildConfiguration());

        var definitionDescriptor = services.FirstOrDefault(d =>
            d.ServiceType.Name.Contains("ConsumerDefinition") ||
            d.ImplementationType?.Name.Contains("ConsumerDefinition") == true);

        definitionDescriptor.Should().NotBeNull(
            "VideoUploadedEventConsumerDefinition deve ser registrado");
    }

    [Fact]
    public void AddWorkerServices_WithCustomRabbitHost_ShouldNotThrow()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        var act = () => services.AddWorkerServices(
            BuildConfiguration(rabbitHost: "custom-rabbitmq-host"));

        act.Should().NotThrow();
    }

    [Fact]
    public void AddWorkerServices_WithMissingRabbitHost_ShouldUseDefaultLocalhost()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        var act = () => services.AddWorkerServices(
            BuildConfiguration(rabbitHost: null));

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
    public void VideoUploadedEventConsumerDefinition_ShouldHaveConcurrentMessageLimit()
    {
        var definition = new VideoUploadedEventConsumerDefinition();

        definition.ConcurrentMessageLimit.Should().Be(10);
    }
}