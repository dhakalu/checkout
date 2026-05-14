using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Identity;

public class GlobalExceptionHandler : IExceptionHandler
{

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (statusCode, message, detail) = exception switch
        {
            ArgumentException argEx =>
                (StatusCodes.Status400BadRequest, "Bad request", argEx.Message),
            _ =>
                (StatusCodes.Status500InternalServerError, "Internal server error", "An unexpected error occurred.")
        };

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/json";

        var errorResponse = new ProblemDetails
        {
            Status = statusCode,
            Title = message,
            Detail = detail,
            Instance = httpContext.Request.Path
        };
        await httpContext.Response.WriteAsJsonAsync(
            errorResponse,
            cancellationToken);
        return await new ValueTask<bool>(true);
    }
}