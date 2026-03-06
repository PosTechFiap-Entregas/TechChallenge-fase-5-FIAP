using FiapX.API.Configurations;
using FluentAssertions;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace FiapX.API.Tests.Configurations;

public class CorsConfigurationTests
{
    private static ServiceCollection BuildServices()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        return services;
    }

    [Fact]
    public void AddCorsConfiguration_ShouldReturnSameServiceCollection()
    {
        var services = BuildServices();

        var result = services.AddCorsConfiguration();

        result.Should().BeSameAs(services);
    }

    [Fact]
    public void AddCorsConfiguration_ShouldRegisterCorsServices()
    {
        var services = BuildServices();
        services.AddCorsConfiguration();
        var serviceProvider = services.BuildServiceProvider();

        var corsService = serviceProvider.GetService<ICorsService>();

        corsService.Should().NotBeNull();
    }

    [Fact]
    public void AddCorsConfiguration_ShouldRegisterCorsPolicyProvider()
    {
        var services = BuildServices();
        services.AddCorsConfiguration();
        var serviceProvider = services.BuildServiceProvider();

        var policyProvider = serviceProvider.GetService<ICorsPolicyProvider>();

        policyProvider.Should().NotBeNull();
    }

    [Fact]
    public void AddCorsConfiguration_CorsPolicy_ShouldAllowAnyOrigin()
    {
        var services = BuildServices();
        services.AddCorsConfiguration();
        var serviceProvider = services.BuildServiceProvider();
        var policyProvider = serviceProvider.GetRequiredService<ICorsPolicyProvider>();

        var policy = policyProvider.GetPolicyAsync(new DefaultHttpContext(), "CorsPolicy").Result;

        policy.Should().NotBeNull();
        policy!.AllowAnyOrigin.Should().BeTrue();
    }

    [Fact]
    public void AddCorsConfiguration_CorsPolicy_ShouldAllowAnyHeader()
    {
        var services = BuildServices();
        services.AddCorsConfiguration();
        var serviceProvider = services.BuildServiceProvider();
        var policyProvider = serviceProvider.GetRequiredService<ICorsPolicyProvider>();

        var policy = policyProvider.GetPolicyAsync(new DefaultHttpContext(), "CorsPolicy").Result;

        policy.Should().NotBeNull();
        policy!.AllowAnyHeader.Should().BeTrue();
    }

    [Fact]
    public void AddCorsConfiguration_CorsPolicy_ShouldAllowAnyMethod()
    {
        var services = BuildServices();
        services.AddCorsConfiguration();
        var serviceProvider = services.BuildServiceProvider();
        var policyProvider = serviceProvider.GetRequiredService<ICorsPolicyProvider>();

        var policy = policyProvider.GetPolicyAsync(new DefaultHttpContext(), "CorsPolicy").Result;

        policy.Should().NotBeNull();
        policy!.AllowAnyMethod.Should().BeTrue();
    }

    [Fact]
    public void AddCorsConfiguration_UnknownPolicy_ShouldReturnNull()
    {
        var services = BuildServices();
        services.AddCorsConfiguration();
        var serviceProvider = services.BuildServiceProvider();
        var policyProvider = serviceProvider.GetRequiredService<ICorsPolicyProvider>();

        var policy = policyProvider.GetPolicyAsync(new DefaultHttpContext(), "UnknownPolicy").Result;

        policy.Should().BeNull();
    }
}