using System.Reflection;
using WebApi.Identity;

public static class Endpoints
{
    public static IEndpointRouteBuilder MapAllEndpoints(this IEndpointRouteBuilder app)
    {
        var endpointTypes = Assembly.GetExecutingAssembly()
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

    public static IServiceCollection AddAllHandlers(this IServiceCollection services)
    {
        var handlerTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => typeof(IHandler).IsAssignableFrom(t) 
                        && !t.IsInterface 
                        && !t.IsAbstract);

        foreach (var type in handlerTypes)
        {
            services.AddScoped(type); 
            services.AddScoped(typeof(IHandler), type); 
        }

        return services;
    }
}
