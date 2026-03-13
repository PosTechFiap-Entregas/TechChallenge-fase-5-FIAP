using FiapX.API.Extensions;
using FiapX.Infrastructure.Persistence.Context;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FiapX.API.Tests.Extensions;

public class MigrationExtensionsTests
{
    private static SqliteConnection CreateOpenConnection()
    {
        var conn = new SqliteConnection("DataSource=:memory:");
        conn.Open();
        return conn;
    }

    private static AppDbContext CreateDbContext(SqliteConnection connection)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;
        return new AppDbContext(options);
    }

    private static AppDbContext CreateDbContextWithSchema(SqliteConnection connection)
    {
        var ctx = CreateDbContext(connection);
        ctx.Database.EnsureCreated();
        return ctx;
    }

    private static (WebApplication App, CaptureLoggerProvider LogCapture) BuildWebAppWithCapture(
        AppDbContext dbContext)
    {
        var capture = new CaptureLoggerProvider();
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("http://localhost:0");
        builder.Services.AddSingleton(dbContext);
        builder.Logging.ClearProviders();
        builder.Logging.AddProvider(capture);
        return (builder.Build(), capture);
    }

    private static WebApplication BuildWebApp(AppDbContext dbContext)
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("http://localhost:0");
        builder.Services.AddSingleton(dbContext);
        builder.Logging.ClearProviders();
        return builder.Build();
    }

    private static (WebApplication App, CaptureLoggerProvider LogCapture) BuildInvalidDbWithCapture()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=invalid_host_does_not_exist;Database=test;Username=x;Password=x;Connect Timeout=1")
            .Options;

        var capture = new CaptureLoggerProvider();
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("http://localhost:0");
        builder.Services.AddSingleton(new AppDbContext(options));
        builder.Logging.ClearProviders();
        builder.Logging.AddProvider(capture);
        return (builder.Build(), capture);
    }

    private static WebApplication BuildInvalidDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=invalid_host_does_not_exist;Database=test;Username=x;Password=x;Connect Timeout=1")
            .Options;

        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("http://localhost:0");
        builder.Services.AddSingleton(new AppDbContext(options));
        builder.Logging.ClearProviders();
        return builder.Build();
    }

    [Fact]
    public async Task ApplyMigrationsAsync_WhenNoPendingMigrations_ShouldNotThrow()
    {
        using var conn = CreateOpenConnection();
        await using var dbContext = CreateDbContext(conn);
        await using var app = BuildWebApp(dbContext);

        var act = async () => await app.ApplyMigrationsAsync();

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ApplyMigrationsAsync_ShouldAlwaysLogVerifyingMessage()
    {
        using var conn = CreateOpenConnection();
        await using var dbContext = CreateDbContext(conn);
        var (app, capture) = BuildWebAppWithCapture(dbContext);
        await using var _ = app;

        await app.ApplyMigrationsAsync();

        capture.Entries
            .Should().Contain(e =>
                e.LogLevel == LogLevel.Information &&
                e.Message.Contains("Verificando migrations pendentes"));
    }

    [Fact]
    public async Task ApplyMigrationsAsync_WhenNoPendingMigrations_ShouldLogNoPendingMessage()
    {
        using var conn = CreateOpenConnection();
        await using (var ctx1 = CreateDbContext(conn))
        await using (var app1 = BuildWebApp(ctx1))
            await app1.ApplyMigrationsAsync();

        await using var ctx2 = CreateDbContext(conn);
        var (app2, capture) = BuildWebAppWithCapture(ctx2);
        await using var _ = app2;

        await app2.ApplyMigrationsAsync();

        capture.Entries
            .Should().Contain(e =>
                e.LogLevel == LogLevel.Information &&
                e.Message.Contains("Nenhuma migration pendente"));
    }

    [Fact]
    public async Task ApplyMigrationsAsync_WhenNoPendingMigrations_ShouldNotLogApplyingMessage()
    {
        using var conn = CreateOpenConnection();
        await using (var ctx1 = CreateDbContext(conn))
        await using (var app1 = BuildWebApp(ctx1))
            await app1.ApplyMigrationsAsync();

        await using var ctx2 = CreateDbContext(conn);
        var (app2, capture) = BuildWebAppWithCapture(ctx2);
        await using var _ = app2;

        await app2.ApplyMigrationsAsync();

        capture.Entries
            .Should().NotContain(e =>
                e.LogLevel == LogLevel.Information &&
                e.Message.Contains("Aplicando"));
    }

    [Fact]
    public async Task ApplyMigrationsAsync_ShouldResolveDbContextFromDI()
    {
        using var conn = CreateOpenConnection();
        await using var dbContext = CreateDbContext(conn);
        await using var app = BuildWebApp(dbContext);

        var act = async () => await app.ApplyMigrationsAsync();
        await act.Should().NotThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task ApplyMigrationsAsync_WhenCalledTwiceOnSameDb_ShouldNotThrow()
    {
        using var conn = CreateOpenConnection();

        await using (var ctx1 = CreateDbContext(conn))
        await using (var app1 = BuildWebApp(ctx1))
            await app1.ApplyMigrationsAsync();

        await using var ctx2 = CreateDbContext(conn);
        await using var app2 = BuildWebApp(ctx2);

        var act = async () => await app2.ApplyMigrationsAsync();
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ApplyMigrationsAsync_WhenDbThrows_ShouldRethrowException()
    {
        await using var app = BuildInvalidDb();

        var act = async () => await app.ApplyMigrationsAsync();

        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task ApplyMigrationsAsync_WhenDbThrows_ShouldLogError()
    {
        var (app, capture) = BuildInvalidDbWithCapture();
        await using var _ = app;

        try { await app.ApplyMigrationsAsync(); } catch { }

        capture.Entries
            .Should().Contain(e =>
                e.LogLevel == LogLevel.Error &&
                e.Message.Contains("Erro ao aplicar migrations"));
    }

    [Fact]
    public async Task ApplyMigrationsAsync_WhenDbThrows_ShouldNotLogSuccessMessage()
    {
        var (app, capture) = BuildInvalidDbWithCapture();
        await using var _ = app;

        try { await app.ApplyMigrationsAsync(); } catch {  }

        capture.Entries
            .Should().NotContain(e =>
                e.LogLevel == LogLevel.Information &&
                e.Message.Contains("sucesso"));
    }

    [Fact]
    public async Task AppDbContext_SaveChangesAsync_WhenNoModifiedEntities_ShouldNotThrow()
    {
        using var conn = CreateOpenConnection();
        await using var dbContext = CreateDbContextWithSchema(conn);

        var act = async () => await dbContext.SaveChangesAsync();

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task AppDbContext_SaveChangesAsync_ShouldReturnZero_WhenNothingChanged()
    {
        using var conn = CreateOpenConnection();
        await using var dbContext = CreateDbContextWithSchema(conn);

        var result = await dbContext.SaveChangesAsync();

        result.Should().Be(0);
    }

    [Fact]
    public async Task AppDbContext_SaveChangesAsync_WithCancellationToken_ShouldNotThrow()
    {
        using var conn = CreateOpenConnection();
        await using var dbContext = CreateDbContextWithSchema(conn);
        using var cts = new CancellationTokenSource();

        var act = async () => await dbContext.SaveChangesAsync(cts.Token);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public void AppDbContext_DbSets_Users_ShouldBeAccessible()
    {
        using var conn = CreateOpenConnection();
        using var dbContext = CreateDbContextWithSchema(conn);

        var act = () => dbContext.Users;

        act.Should().NotThrow();
        dbContext.Users.Should().NotBeNull();
    }

    [Fact]
    public void AppDbContext_DbSets_Videos_ShouldBeAccessible()
    {
        using var conn = CreateOpenConnection();
        using var dbContext = CreateDbContextWithSchema(conn);

        var act = () => dbContext.Videos;

        act.Should().NotThrow();
        dbContext.Videos.Should().NotBeNull();
    }
}

internal sealed record LogEntry(LogLevel LogLevel, string Message);

internal sealed class CaptureLoggerProvider : ILoggerProvider
{
    private readonly List<LogEntry> _entries = new();

    public IReadOnlyList<LogEntry> Entries => _entries;

    public ILogger CreateLogger(string categoryName) =>
        new CaptureLogger(categoryName, _entries);

    public void Dispose() { }
}

internal sealed class CaptureLogger : ILogger
{
    private readonly string _category;
    private readonly List<LogEntry> _entries;

    public CaptureLogger(string category, List<LogEntry> entries)
    {
        _category = category;
        _entries = entries;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        var message = formatter(state, exception);
        _entries.Add(new LogEntry(logLevel, message));
    }
}