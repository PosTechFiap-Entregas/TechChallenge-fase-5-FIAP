using FiapX.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace FiapX.API.Extensions;

/// <summary>
/// Extensões para aplicar migrations automaticamente no banco de dados
/// </summary>
public static class MigrationExtensions
{
    /// <summary>
    /// Aplica migrations pendentes no banco de dados automaticamente
    /// </summary>
    public static async Task ApplyMigrationsAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        try
        {
            logger.LogInformation("Verificando migrations pendentes...");

            var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();

            if (pendingMigrations.Any())
            {
                logger.LogInformation("Aplicando {Count} migrations pendentes", pendingMigrations.Count());
                await dbContext.Database.MigrateAsync();
                logger.LogInformation("Migrations aplicadas com sucesso!");
            }
            else
            {
                logger.LogInformation("Nenhuma migration pendente");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao aplicar migrations");
            throw;
        }
    }
}