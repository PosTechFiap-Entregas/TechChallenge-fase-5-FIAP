using FiapX.API.Configurations;
using FiapX.API.Middlewares;
using FiapX.Application.Extensions;
using FiapX.Infrastructure.Extensions;
using FiapX.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilogConfiguration();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerConfiguration();
builder.Services.AddJwtConfiguration(builder.Configuration);
builder.Services.AddCorsConfiguration();
builder.Services.AddHealthChecksConfiguration(builder.Configuration);
builder.Services.AddUploadConfiguration();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
app.UseSerilogRequestLogging();

//if (app.Environment.IsDevelopment())
//{
//    app.UseSwaggerConfiguration();
//}

app.UseSwaggerConfiguration();

app.UseCors("CorsPolicy");
app.UseAuthentication();
app.UseAuthorization();

app.UseHealthChecksConfiguration();
app.MapControllers();

Log.Information("FiapX API iniciada com sucesso.");

// Aplicar Migrations Automaticamente
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        var logger = services.GetRequiredService<ILogger<Program>>();

        logger.LogInformation("Verificando e aplicando migrations...");
        context.Database.Migrate();
        logger.LogInformation("Migrations aplicadas com sucesso!");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Erro ao aplicar migrations");
        throw;
    }
}

app.Run();