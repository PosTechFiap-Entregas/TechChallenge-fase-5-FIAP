using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace FiapX.API.Configurations;

/// <summary>
/// Configurações do Swagger / OpenAPI
/// </summary>
public static class SwaggerConfiguration
{
    public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "FiapX - Video Processing API",
                Version = "v1",
                Description = "API para processamento de vídeos em frames",
                Contact = new OpenApiContact
                {
                    Name = "FIAP TechChallenge",
                    Email = "fiapx@fiap.com.br"
                },
                License = new OpenApiLicense
                {
                    Name = "MIT"
                }
            });

            // Suporte a JWT no Swagger
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "Insira o token JWT.\r\n\r\nExemplo: Bearer {token}",
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        return services;
    }

    public static WebApplication UseSwaggerConfiguration(this WebApplication app)
    {
        app.UseSwagger();

        app.UseSwaggerUI();

        //app.UseSwaggerUI(options =>
        //{
        //    options.SwaggerEndpoint("/swagger/v1/swagger.json", "FiapX API v1");
        //    options.RoutePrefix = "docs";
        //});

        return app;
    }
}