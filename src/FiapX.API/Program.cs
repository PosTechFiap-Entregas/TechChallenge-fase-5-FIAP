using FiapX.API.Configurations;
using FiapX.API.Middlewares;
using FiapX.Application.Extensions;
using FiapX.Infrastructure.Extensions;
using FiapX.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Prometheus;
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
app.UseSwaggerConfiguration();

app.UseCors("CorsPolicy");

app.UseHttpMetrics(options =>
{
    options.AddCustomLabel("host", context => context.Request.Host.Host);
});

app.MapMetrics();
app.UseAuthentication();
app.UseAuthorization();

app.UseHealthChecksConfiguration();
app.MapControllers();

Log.Information("FiapX API iniciada com sucesso.");

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