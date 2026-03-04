using FiapX.Application.Interfaces.UseCases;
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
        services.AddScoped<IRegisterUserUseCase, RegisterUserUseCase>();
        services.AddScoped<ILoginUseCase, LoginUseCase>();

        // Use Cases - Videos
        services.AddScoped<IUploadVideoUseCase, UploadVideoUseCase>();
        services.AddScoped<IGetUserVideosUseCase, GetUserVideosUseCase>();
        services.AddScoped<IGetVideoStatusUseCase, GetVideoStatusUseCase>();
        services.AddScoped<IDownloadVideoUseCase, DownloadVideoUseCase>();

        return services;
    }
}