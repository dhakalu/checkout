using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Identity;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger = logger;

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "An unhandled exception occurred while processing the request.");

        if (exception is ValidationException validationException)
        {
            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            httpContext.Response.ContentType = "application/json";


            var errors = validationException.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => System.Text.Json.JsonNamingPolicy.CamelCase.ConvertName(g.Key),
                    g => g.Select(e => e.ErrorMessage).ToArray()
                );

            var validationProblemDetails = new ValidationProblemDetails(errors)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation error",
                Detail = "One or more validation errors occurred.",
                Instance = httpContext.Request.Path
            };
            await httpContext.Response.WriteAsJsonAsync(
                validationProblemDetails,
                cancellationToken);
            return true;
        }

        var (statusCode, message, detail) = (200, "", "");
        if (exception is BadHttpRequestException badHttpRequestException)
        {
            string clientFriendlyMessage = "One or more parameters provided in the request URL are malformed or invalid.";

            var env = httpContext.RequestServices.GetRequiredService<IWebHostEnvironment>();
            if (env.IsDevelopment() || env.IsEnvironment("Testing"))
            {
                clientFriendlyMessage = badHttpRequestException.Message;
            }
            (statusCode, message, detail) = (StatusCodes.Status400BadRequest, "Bad Request", clientFriendlyMessage);
        }
        else
        {
            (statusCode, message, detail) = exception switch
            {
                JsonException jsonException =>
                    (StatusCodes.Status400BadRequest, "Bad request", jsonException.Message),
                ArgumentException argEx =>
                    (StatusCodes.Status400BadRequest, "Bad request", argEx.Message),
                KeyNotFoundException kEx => (
                    StatusCodes.Status404NotFound, "Not found", kEx.Message),
                InvalidOperationException ioEx => (
                    StatusCodes.Status409Conflict, "Conflict", ioEx.Message),
                UnauthorizedAccessException uEx => (
                    StatusCodes.Status401Unauthorized, "Unauthorized", uEx.Message
                ),
                _ =>
                    (StatusCodes.Status500InternalServerError, "Internal server error", "An unexpected error occurred.")
            };
        }



        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/json";

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = message,
            Detail = detail,
            Instance = httpContext.Request.Path
        };
        await httpContext.Response.WriteAsJsonAsync(
            problemDetails,
            cancellationToken);
        return true;
    }
}