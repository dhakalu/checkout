using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;

namespace Shared.DependencyInjection;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger = logger;

    private static string ConvertPathToCamelCase(string path)
    {
        if (string.IsNullOrEmpty(path)) return path;

        // Splits paths like "Items[0].ProductId" into ["Items[0]", "ProductId"]
        var segments = path.Split('.');

        for (int i = 0; i < segments.Length; i++)
        {
            var segment = segments[i];

            // Checks if the segment contains an array index like "Items[0]"
            if (segment.Contains('[') && segment.EndsWith(']'))
            {
                var openBracketIndex = segment.IndexOf('[');
                var propertyName = segment.Substring(0, openBracketIndex);
                var indexPart = segment.Substring(openBracketIndex); // Keeps "[0]"

                // Converts just the property name to camelCase and reattaches the index
                segments[i] = JsonNamingPolicy.CamelCase.ConvertName(propertyName) + indexPart;
            }
            else
            {
                // Converts standard non-indexed properties to camelCase
                segments[i] = JsonNamingPolicy.CamelCase.ConvertName(segment);
            }
        }

        return string.Join(".", segments);
    }

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
                    g => ConvertPathToCamelCase(g.Key),
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