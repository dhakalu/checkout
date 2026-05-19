using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Shared.DependencyInjection;

public static class DatabaseSetup
{

    public static IServiceCollection AddDb<T>(this IServiceCollection services, IConfiguration configuration, Assembly assembly) where T : DbContext
    {
        services.AddDbContext<T>(options =>
        {
            options.EnableSensitiveDataLogging();
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"), (npgsqlOptionsAction) =>
            {
                npgsqlOptionsAction.EnableRetryOnFailure(3);
            });
        });
        services.AddAllRepositories(assembly);
        return services;
    }

    private static IServiceCollection AddAllRepositories(this IServiceCollection services, Assembly assembly)
    {
        var repoClasses = assembly
            .GetTypes()
            .Where(t => typeof(IRepository).IsAssignableFrom(t)
                        && !t.IsInterface
                        && !t.IsAbstract);

        foreach (var repoClass in repoClasses)
        {
            services.AddScoped(repoClass);
            // services.AddScoped(typeof(IRepository), repoClass);
        }

        return services;
    }

}
