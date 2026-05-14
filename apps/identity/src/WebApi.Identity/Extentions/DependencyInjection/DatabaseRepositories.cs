namespace WebApi.Identity.Extentions.DependencyInjection;

public static class DatabaseRepositories
{

    public static IServiceCollection AddDbRepositories(this IServiceCollection services)
    {
        services.AddScoped<Features.Signup.Data.IdentityRepository>();
        return services;
    }

}
