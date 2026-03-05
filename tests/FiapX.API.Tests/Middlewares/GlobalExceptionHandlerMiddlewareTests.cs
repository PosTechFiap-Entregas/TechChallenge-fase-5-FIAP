using FiapX.API.Middlewares;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;

namespace FiapX.API.Tests.Middlewares;

public class GlobalExceptionHandlerMiddlewareTests
{
    private readonly Mock<ILogger<GlobalExceptionHandlerMiddleware>> _loggerMock;
    private readonly GlobalExceptionHandlerMiddleware _middleware;

    public GlobalExceptionHandlerMiddlewareTests()
    {
        _loggerMock = new Mock<ILogger<GlobalExceptionHandlerMiddleware>>();
        _middleware = new GlobalExceptionHandlerMiddleware(
            next: (innerHttpContext) => Task.CompletedTask,
            logger: _loggerMock.Object);
    }

    [Fact]
    public async Task InvokeAsync_WhenNoExceptionOccurs_ShouldCallNext()
    {
        var context = new DefaultHttpContext();
        var nextCalled = false;

        var middleware = new GlobalExceptionHandlerMiddleware(
            next: (innerHttpContext) =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            },
            logger: _loggerMock.Object);

        await middleware.InvokeAsync(context);

        nextCalled.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
    }

    [Fact]
    public async Task InvokeAsync_WhenUnauthorizedAccessExceptionOccurs_ShouldReturn401()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var middleware = new GlobalExceptionHandlerMiddleware(
            next: (innerHttpContext) => throw new UnauthorizedAccessException("Não autorizado"),
            logger: _loggerMock.Object);

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        context.Response.ContentType.Should().StartWith("application/json");

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();

        var jsonDoc = JsonDocument.Parse(responseBody);
        var message = jsonDoc.RootElement.GetProperty("message").GetString();
        message.Should().Be("Não autorizado.");
    }

    [Fact]
    public async Task InvokeAsync_WhenArgumentExceptionOccurs_ShouldReturn400()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var middleware = new GlobalExceptionHandlerMiddleware(
            next: (innerHttpContext) => throw new ArgumentException("Argumento inválido"),
            logger: _loggerMock.Object);

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();

        var jsonDoc = JsonDocument.Parse(responseBody);
        var message = jsonDoc.RootElement.GetProperty("message").GetString();
        message.Should().Be("Argumento inválido");
    }

    [Fact]
    public async Task InvokeAsync_WhenInvalidOperationExceptionOccurs_ShouldReturn400()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var middleware = new GlobalExceptionHandlerMiddleware(
            next: (innerHttpContext) => throw new InvalidOperationException("Operação inválida"),
            logger: _loggerMock.Object);

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();

        var jsonDoc = JsonDocument.Parse(responseBody);
        var message = jsonDoc.RootElement.GetProperty("message").GetString();
        message.Should().Be("Operação inválida");
    }

    [Fact]
    public async Task InvokeAsync_WhenFileNotFoundExceptionOccurs_ShouldReturn404()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var middleware = new GlobalExceptionHandlerMiddleware(
            next: (innerHttpContext) => throw new FileNotFoundException("Arquivo não encontrado"),
            logger: _loggerMock.Object);

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status404NotFound);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();

        var jsonDoc = JsonDocument.Parse(responseBody);
        var message = jsonDoc.RootElement.GetProperty("message").GetString();
        message.Should().Be("Recurso não encontrado.");
    }

    [Fact]
    public async Task InvokeAsync_WhenUnknownExceptionOccurs_ShouldReturn500()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var middleware = new GlobalExceptionHandlerMiddleware(
            next: (innerHttpContext) => throw new Exception("Erro desconhecido"),
            logger: _loggerMock.Object);

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();

        var jsonDoc = JsonDocument.Parse(responseBody);
        var message = jsonDoc.RootElement.GetProperty("message").GetString();
        message.Should().Be("Um erro interno ocorreu.");
    }

    [Fact]
    public async Task InvokeAsync_WhenExceptionOccurs_ShouldLogError()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        context.Request.Method = "GET";
        context.Request.Path = "/api/test";

        var middleware = new GlobalExceptionHandlerMiddleware(
            next: (innerHttpContext) => throw new Exception("Test exception"),
            logger: _loggerMock.Object);

        await middleware.InvokeAsync(context);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Erro não tratado")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}