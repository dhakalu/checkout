namespace WebApi.Identity.Extentions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

public static class DatabaseSetup
{

    public static IServiceCollection AddDb(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<IdentityDbContext>(options =>
        {
            options.EnableSensitiveDataLogging();
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
        });
        services.AddScoped<Features.Users.Data.UserRepository>();
        services.AddScoped<Features.Clients.Data.ClientRepository>();
        return services;
    }

}
