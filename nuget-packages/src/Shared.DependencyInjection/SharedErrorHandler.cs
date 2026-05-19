using System.Reflection;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Shared.DependencyInjection;

public static class SharedErrorHandler
{

    public static IServiceCollection AddSharedErrorHandler(this IServiceCollection services, Assembly serviceAssembly)
    {
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        // Automatically scans the calling microservice assembly for all AbstractValidator implementations
        services.AddValidatorsFromAssembly(serviceAssembly);
        return services;
    }

    public static IApplicationBuilder UseSharedErrorHandling(this IApplicationBuilder app)
    {
        app.UseExceptionHandler();
        return app;
    }

}
