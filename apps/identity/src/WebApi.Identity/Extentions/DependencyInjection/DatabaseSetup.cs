namespace WebApi.Identity.Extentions.DependencyInjection;

using Microsoft.EntityFrameworkCore;
using WebApi.Identity.Features.Clients.Data;
using WebApi.Identity.Features.Scopes.Data;
using WebApi.Identity.Features.Users.Data;

public static class DatabaseSetup
{

    public static IServiceCollection AddDb(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<IdentityDbContext>(options =>
        {
            options.EnableSensitiveDataLogging();
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
        });
        services.AddScoped<UserRepository>();
        services.AddScoped<ClientRepository>();
        services.AddScoped<ScopeRepository>();
        return services;
    }

}
