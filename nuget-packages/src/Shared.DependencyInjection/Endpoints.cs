using System.Reflection;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;


namespace Shared.DependencyInjection;

public static class Endpoints
{
    public static IEndpointRouteBuilder MapAllEndpoints(this IEndpointRouteBuilder app, Assembly assembly)
    {
        var endpointTypes = assembly
            .GetTypes()
            .Where(t => typeof(IEndpoint).IsAssignableFrom(t)
                        && !t.IsInterface
                        && !t.IsAbstract);

        foreach (var type in endpointTypes)
        {
            // If using a parameterless constructor instance approach:
            if (Activator.CreateInstance(type) is IEndpoint endpoint)
            {
                endpoint.MapEndpoints(app);
            }
        }

        return app;
    }

    public static IServiceCollection AddAllHandlers(this IServiceCollection services, Assembly assembly)
    {
        var handlerClasses = assembly
            .GetTypes()
            .Where(t => typeof(IHandler).IsAssignableFrom(t)
                        && !t.IsInterface
                        && !t.IsAbstract);

        foreach (var handlerClass in handlerClasses)
        {
            services.AddScoped(handlerClass);
        }

        return services;
    }
}
