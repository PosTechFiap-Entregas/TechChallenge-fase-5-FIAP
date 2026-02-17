using FiapX.Application.UseCases.Auth;
using FiapX.Application.UseCases.Videos;
using FiapX.Application.Validators;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace FiapX.Application.Extensions;

/// <summary>
/// Extensões de DI para a camada Application
/// </summary>
public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Validators (FluentValidation)
        services.AddValidatorsFromAssemblyContaining<RegisterUserRequestValidator>();

        // Use Cases - Auth
        services.AddScoped<RegisterUserUseCase>();
        services.AddScoped<LoginUseCase>();

        // Use Cases - Videos
        services.AddScoped<UploadVideoUseCase>();
        services.AddScoped<GetUserVideosUseCase>();
        services.AddScoped<GetVideoStatusUseCase>();
        services.AddScoped<DownloadVideoUseCase>();

        return services;
    }
}