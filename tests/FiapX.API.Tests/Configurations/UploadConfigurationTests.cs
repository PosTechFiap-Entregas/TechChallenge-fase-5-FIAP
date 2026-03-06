using FiapX.API.Configurations;
using FluentAssertions;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FiapX.API.Tests.Configurations;

public class UploadConfigurationTests
{
    private const long ExpectedMaxFileSizeBytes = 2L * 1024 * 1024 * 1024;

    [Fact]
    public void AddUploadConfiguration_ShouldReturnSameServiceCollection()
    {
        var services = new ServiceCollection();

        var result = services.AddUploadConfiguration();

        result.Should().BeSameAs(services);
    }

    [Fact]
    public void AddUploadConfiguration_ShouldRegisterFormOptions()
    {
        var services = new ServiceCollection();

        services.AddUploadConfiguration();
        var serviceProvider = services.BuildServiceProvider();

        var formOptions = serviceProvider.GetService<IOptions<FormOptions>>();
        formOptions.Should().NotBeNull();
    }

    [Fact]
    public void AddUploadConfiguration_FormOptions_ShouldSet2GBLimit()
    {
        var services = new ServiceCollection();

        services.AddUploadConfiguration();
        var serviceProvider = services.BuildServiceProvider();

        var formOptions = serviceProvider.GetRequiredService<IOptions<FormOptions>>().Value;
        formOptions.MultipartBodyLengthLimit.Should().Be(ExpectedMaxFileSizeBytes);
    }

    [Fact]
    public void AddUploadConfiguration_KestrelOptions_ShouldSet2GBLimit()
    {
        var services = new ServiceCollection();

        services.AddUploadConfiguration();
        var serviceProvider = services.BuildServiceProvider();

        var kestrelOptions = serviceProvider.GetRequiredService<IOptions<KestrelServerOptions>>().Value;
        kestrelOptions.Limits.MaxRequestBodySize.Should().Be(ExpectedMaxFileSizeBytes);
    }

    [Fact]
    public void AddUploadConfiguration_ShouldRegisterKestrelOptions()
    {
        var services = new ServiceCollection();

        services.AddUploadConfiguration();
        var serviceProvider = services.BuildServiceProvider();

        var kestrelOptions = serviceProvider.GetService<IOptions<KestrelServerOptions>>();
        kestrelOptions.Should().NotBeNull();
    }

    [Fact]
    public void AddUploadConfiguration_MultipartBodyLengthLimit_ShouldBeExactly2GB()
    {
        var services = new ServiceCollection();
        services.AddUploadConfiguration();
        var serviceProvider = services.BuildServiceProvider();

        var formOptions = serviceProvider.GetRequiredService<IOptions<FormOptions>>().Value;

        formOptions.MultipartBodyLengthLimit.Should().Be(2_147_483_648L);
    }

    [Fact]
    public void AddUploadConfiguration_MaxRequestBodySize_ShouldBeExactly2GB()
    {
        var services = new ServiceCollection();
        services.AddUploadConfiguration();
        var serviceProvider = services.BuildServiceProvider();

        var kestrelOptions = serviceProvider.GetRequiredService<IOptions<KestrelServerOptions>>().Value;

        kestrelOptions.Limits.MaxRequestBodySize.Should().Be(2_147_483_648L);
    }
}