using System.Net;
using System.Text.Json;
using FluentValidation;
using ServiceHubPro.Application.Common.Models;

namespace ServiceHubPro.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var code = HttpStatusCode.InternalServerError;
        var errors = new List<string>();

        switch (exception)
        {
            case ValidationException validationException:
                code = HttpStatusCode.BadRequest;
                errors.AddRange(validationException.Errors.Select(e => e.ErrorMessage));
                break;
            case UnauthorizedAccessException:
                code = HttpStatusCode.Unauthorized;
                errors.Add("Unauthorized access");
                break;
            case KeyNotFoundException:
                code = HttpStatusCode.NotFound;
                errors.Add("Resource not found");
                break;
            default:
                errors.Add("An error occurred while processing your request");
                break;
        }

        var result = JsonSerializer.Serialize(ApiResponse<object>.ErrorResponse(
            exception.Message,
            errors
        ), new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)code;

        return context.Response.WriteAsync(result);
    }
}
