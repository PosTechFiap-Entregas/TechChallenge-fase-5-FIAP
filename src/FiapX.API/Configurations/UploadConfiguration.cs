using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace FiapX.API.Configurations;

/// <summary>
/// Configurações para upload de arquivos grandes (até 2GB)
/// </summary>
public static class UploadConfiguration
{
    private const long MaxFileSizeBytes = 2L * 1024 * 1024 * 1024; // 2GB

    public static IServiceCollection AddUploadConfiguration(this IServiceCollection services)
    {
        services.Configure<FormOptions>(options =>
        {
            options.MultipartBodyLengthLimit = MaxFileSizeBytes;
        });

        services.Configure<KestrelServerOptions>(options =>
        {
            options.Limits.MaxRequestBodySize = MaxFileSizeBytes;
        });

        return services;
    }
}