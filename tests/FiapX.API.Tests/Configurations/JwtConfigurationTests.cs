using FiapX.API.Configurations;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FiapX.API.Tests.Configurations;

public class JwtConfigurationTests
{
    private static IConfiguration BuildConfiguration(
        string? secret = "super_secret_key_for_testing_that_is_long_enough_32chars",
        string? issuer = "FiapX",
        string? audience = "FiapXUsers")
    {
        var settings = new Dictionary<string, string?>();
        if (secret != null) settings["JWT:Secret"] = secret;
        if (issuer != null) settings["JWT:Issuer"] = issuer;
        if (audience != null) settings["JWT:Audience"] = audience;

        return new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();
    }

    private static ServiceCollection BuildServices()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        return services;
    }

    [Fact]
    public void AddJwtConfiguration_ShouldReturnSameServiceCollection()
    {
        var services = BuildServices();

        var result = services.AddJwtConfiguration(BuildConfiguration());

        result.Should().BeSameAs(services);
    }

    [Fact]
    public void AddJwtConfiguration_ShouldRegisterAuthenticationServices()
    {
        var services = BuildServices();
        services.AddJwtConfiguration(BuildConfiguration());
        var serviceProvider = services.BuildServiceProvider();

        var authService = serviceProvider.GetService<IAuthenticationService>();

        authService.Should().NotBeNull();
    }

    [Fact]
    public void AddJwtConfiguration_ShouldRegisterAuthorizationServices()
    {
        var services = BuildServices();
        services.AddJwtConfiguration(BuildConfiguration());
        var serviceProvider = services.BuildServiceProvider();

        var authorizationService = serviceProvider
            .GetService<Microsoft.AspNetCore.Authorization.IAuthorizationService>();

        authorizationService.Should().NotBeNull();
    }

    [Fact]
    public async Task AddJwtConfiguration_ShouldConfigureJwtBearerAsDefaultScheme()
    {
        var services = BuildServices();
        services.AddJwtConfiguration(BuildConfiguration());
        var serviceProvider = services.BuildServiceProvider();

        var schemeProvider = serviceProvider
            .GetRequiredService<IAuthenticationSchemeProvider>();

        var defaultScheme = await schemeProvider.GetDefaultAuthenticateSchemeAsync();

        defaultScheme.Should().NotBeNull();
        defaultScheme!.Name.Should().Be(JwtBearerDefaults.AuthenticationScheme);
    }

    [Fact]
    public void AddJwtConfiguration_ShouldConfigureValidIssuer()
    {
        var services = BuildServices();
        services.AddJwtConfiguration(BuildConfiguration(issuer: "TestIssuer"));
        var serviceProvider = services.BuildServiceProvider();

        var jwtOptions = serviceProvider
            .GetRequiredService<IOptionsMonitor<JwtBearerOptions>>()
            .Get(JwtBearerDefaults.AuthenticationScheme);

        jwtOptions.TokenValidationParameters.ValidIssuer.Should().Be("TestIssuer");
    }

    [Fact]
    public void AddJwtConfiguration_ShouldConfigureValidAudience()
    {
        var services = BuildServices();
        services.AddJwtConfiguration(BuildConfiguration(audience: "TestAudience"));
        var serviceProvider = services.BuildServiceProvider();

        var jwtOptions = serviceProvider
            .GetRequiredService<IOptionsMonitor<JwtBearerOptions>>()
            .Get(JwtBearerDefaults.AuthenticationScheme);

        jwtOptions.TokenValidationParameters.ValidAudience.Should().Be("TestAudience");
    }

    [Fact]
    public void AddJwtConfiguration_ShouldEnableLifetimeValidation()
    {
        var services = BuildServices();
        services.AddJwtConfiguration(BuildConfiguration());
        var serviceProvider = services.BuildServiceProvider();

        var jwtOptions = serviceProvider
            .GetRequiredService<IOptionsMonitor<JwtBearerOptions>>()
            .Get(JwtBearerDefaults.AuthenticationScheme);

        jwtOptions.TokenValidationParameters.ValidateLifetime.Should().BeTrue();
    }

    [Fact]
    public void AddJwtConfiguration_ShouldEnableIssuerSigningKeyValidation()
    {
        var services = BuildServices();
        services.AddJwtConfiguration(BuildConfiguration());
        var serviceProvider = services.BuildServiceProvider();

        var jwtOptions = serviceProvider
            .GetRequiredService<IOptionsMonitor<JwtBearerOptions>>()
            .Get(JwtBearerDefaults.AuthenticationScheme);

        jwtOptions.TokenValidationParameters.ValidateIssuerSigningKey.Should().BeTrue();
    }

    [Fact]
    public void AddJwtConfiguration_ShouldSetClockSkewToFiveMinutes()
    {
        var services = BuildServices();
        services.AddJwtConfiguration(BuildConfiguration());
        var serviceProvider = services.BuildServiceProvider();

        var jwtOptions = serviceProvider
            .GetRequiredService<IOptionsMonitor<JwtBearerOptions>>()
            .Get(JwtBearerDefaults.AuthenticationScheme);

        jwtOptions.TokenValidationParameters.ClockSkew.Should().Be(TimeSpan.FromMinutes(5));
    }

    [Fact]
    public void AddJwtConfiguration_WhenSecretIsNull_ShouldThrowInvalidOperationException()
    {
        var services = BuildServices();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JWT:Issuer"] = "FiapX",
                ["JWT:Audience"] = "FiapXUsers"
            })
            .Build();

        var act = () => services.AddJwtConfiguration(configuration);

        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*JWT:Secret*");
    }

    [Fact]
    public async Task AddJwtConfiguration_OnAuthenticationFailed_ShouldAppendTokenExpiredHeader_WhenExpiredException()
    {
        var services = BuildServices();
        services.AddJwtConfiguration(BuildConfiguration());
        var serviceProvider = services.BuildServiceProvider();

        var jwtOptions = serviceProvider
            .GetRequiredService<IOptionsMonitor<JwtBearerOptions>>()
            .Get(JwtBearerDefaults.AuthenticationScheme);

        var httpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();

        var failedContext = new Microsoft.AspNetCore.Authentication.JwtBearer.AuthenticationFailedContext(
            httpContext,
            new AuthenticationScheme(
                JwtBearerDefaults.AuthenticationScheme,
                null,
                typeof(JwtBearerHandler)),
            jwtOptions)
        {
            Exception = new Microsoft.IdentityModel.Tokens.SecurityTokenExpiredException("Token expirado")
        };

        await jwtOptions.Events.OnAuthenticationFailed(failedContext);

        httpContext.Response.Headers.Should().ContainKey("Token-Expired");
        httpContext.Response.Headers["Token-Expired"].ToString().Should().Be("true");
    }

    [Fact]
    public async Task AddJwtConfiguration_OnAuthenticationFailed_ShouldNotAppendTokenExpiredHeader_WhenOtherException()
    {
        var services = BuildServices();
        services.AddJwtConfiguration(BuildConfiguration());
        var serviceProvider = services.BuildServiceProvider();

        var jwtOptions = serviceProvider
            .GetRequiredService<IOptionsMonitor<JwtBearerOptions>>()
            .Get(JwtBearerDefaults.AuthenticationScheme);

        var httpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();

        var failedContext = new Microsoft.AspNetCore.Authentication.JwtBearer.AuthenticationFailedContext(
            httpContext,
            new AuthenticationScheme(
                JwtBearerDefaults.AuthenticationScheme,
                null,
                typeof(JwtBearerHandler)),
            jwtOptions)
        {
            Exception = new Exception("Outro erro qualquer")
        };

        await jwtOptions.Events.OnAuthenticationFailed(failedContext);

        httpContext.Response.Headers.Should().NotContainKey("Token-Expired");
    }
}