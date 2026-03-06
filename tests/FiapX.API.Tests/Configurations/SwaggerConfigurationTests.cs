using FiapX.API.Configurations;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace FiapX.API.Tests.Configurations;

public class SwaggerConfigurationTests
{
    private static ServiceCollection BuildServices()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMvcCore();
        return services;
    }

    [Fact]
    public void AddSwaggerConfiguration_ShouldReturnSameServiceCollection()
    {
        var services = new ServiceCollection();

        var result = services.AddSwaggerConfiguration();

        result.Should().BeSameAs(services);
    }

    [Fact]
    public void AddSwaggerConfiguration_ShouldRegisterSwaggerGenOptions()
    {
        var services = BuildServices();
        services.AddSwaggerConfiguration();
        var serviceProvider = services.BuildServiceProvider();

        var options = serviceProvider.GetService<IOptions<SwaggerGenOptions>>();

        options.Should().NotBeNull();
    }

    [Fact]
    public void AddSwaggerConfiguration_ShouldRegisterSwaggerDocument_WithV1()
    {
        var services = BuildServices();
        services.AddSwaggerConfiguration();
        var serviceProvider = services.BuildServiceProvider();

        var swaggerOptions = serviceProvider
            .GetRequiredService<IOptions<SwaggerGenOptions>>().Value;

        swaggerOptions.SwaggerGeneratorOptions.SwaggerDocs.Should().ContainKey("v1");
    }

    [Fact]
    public void AddSwaggerConfiguration_ShouldSetCorrectApiTitle()
    {
        var services = BuildServices();
        services.AddSwaggerConfiguration();
        var serviceProvider = services.BuildServiceProvider();

        var doc = serviceProvider
            .GetRequiredService<IOptions<SwaggerGenOptions>>().Value
            .SwaggerGeneratorOptions.SwaggerDocs["v1"];

        doc.Title.Should().Be("FiapX - Video Processing API");
    }

    [Fact]
    public void AddSwaggerConfiguration_ShouldSetCorrectApiVersion()
    {
        var services = BuildServices();
        services.AddSwaggerConfiguration();
        var serviceProvider = services.BuildServiceProvider();

        var doc = serviceProvider
            .GetRequiredService<IOptions<SwaggerGenOptions>>().Value
            .SwaggerGeneratorOptions.SwaggerDocs["v1"];

        doc.Version.Should().Be("v1");
    }

    [Fact]
    public void AddSwaggerConfiguration_ShouldSetContactEmail()
    {
        var services = BuildServices();
        services.AddSwaggerConfiguration();
        var serviceProvider = services.BuildServiceProvider();

        var doc = serviceProvider
            .GetRequiredService<IOptions<SwaggerGenOptions>>().Value
            .SwaggerGeneratorOptions.SwaggerDocs["v1"];

        doc.Contact.Should().NotBeNull();
        doc.Contact!.Email.Should().Be("fiapx@fiap.com.br");
    }

    [Fact]
    public void AddSwaggerConfiguration_ShouldIncludeBearerSecurityDefinition()
    {
        var services = BuildServices();
        services.AddSwaggerConfiguration();
        var serviceProvider = services.BuildServiceProvider();

        var swaggerOptions = serviceProvider
            .GetRequiredService<IOptions<SwaggerGenOptions>>().Value;

        swaggerOptions.SwaggerGeneratorOptions.SecuritySchemes
            .Should().ContainKey("Bearer");
    }

    [Fact]
    public void AddSwaggerConfiguration_BearerScheme_ShouldBeHttpType()
    {
        var services = BuildServices();
        services.AddSwaggerConfiguration();
        var serviceProvider = services.BuildServiceProvider();

        var bearerScheme = serviceProvider
            .GetRequiredService<IOptions<SwaggerGenOptions>>().Value
            .SwaggerGeneratorOptions.SecuritySchemes["Bearer"];

        bearerScheme.Type.Should().Be(SecuritySchemeType.Http);
        bearerScheme.Scheme.Should().Be("Bearer");
    }

    [Fact]
    public void AddSwaggerConfiguration_ShouldSetMitLicense()
    {
        var services = BuildServices();
        services.AddSwaggerConfiguration();
        var serviceProvider = services.BuildServiceProvider();

        var doc = serviceProvider
            .GetRequiredService<IOptions<SwaggerGenOptions>>().Value
            .SwaggerGeneratorOptions.SwaggerDocs["v1"];

        doc.License.Should().NotBeNull();
        doc.License!.Name.Should().Be("MIT");
    }

    [Fact]
    public void AddSwaggerConfiguration_ShouldSetApiDescription()
    {
        var services = BuildServices();
        services.AddSwaggerConfiguration();
        var serviceProvider = services.BuildServiceProvider();

        var doc = serviceProvider
            .GetRequiredService<IOptions<SwaggerGenOptions>>().Value
            .SwaggerGeneratorOptions.SwaggerDocs["v1"];

        doc.Description.Should().NotBeNullOrEmpty();
    }
}