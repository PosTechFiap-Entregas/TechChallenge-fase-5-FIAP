using System.Text.Json;

namespace FiapX.API.Middlewares;

public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro não tratado ocorreu na requisição {Method} {Path}",
                context.Request.Method, context.Request.Path);

            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, message) = exception switch
        {
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Não autorizado."),
            ArgumentException argEx => (StatusCodes.Status400BadRequest, argEx.Message),
            InvalidOperationException invEx => (StatusCodes.Status400BadRequest, invEx.Message),
            FileNotFoundException => (StatusCodes.Status404NotFound, "Recurso não encontrado."),
            _ => (StatusCodes.Status500InternalServerError, "Um erro interno ocorreu.")
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var response = new
        {
            Success = false,
            Message = message,
            StatusCode = statusCode
        };

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        await context.Response.WriteAsJsonAsync(response, options);
    }
}