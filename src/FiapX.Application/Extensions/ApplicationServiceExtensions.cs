using FiapX.Application.Interfaces.UseCases;
using FiapX.Application.UseCases.Auth;
using FiapX.Application.UseCases.Videos;
using FiapX.Application.Validators;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace FiapX.Application.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<RegisterUserRequestValidator>();

        services.AddScoped<IRegisterUserUseCase, RegisterUserUseCase>();
        services.AddScoped<ILoginUseCase, LoginUseCase>();
        services.AddScoped<IUploadVideoUseCase, UploadVideoUseCase>();
        services.AddScoped<IGetUserVideosUseCase, GetUserVideosUseCase>();
        services.AddScoped<IGetVideoStatusUseCase, GetVideoStatusUseCase>();
        services.AddScoped<IDownloadVideoUseCase, DownloadVideoUseCase>();

        return services;
    }
}