using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.PostgreSql;
using Microsoft.EntityFrameworkCore;

namespace WebApi.Identity.Tests;

public class IdentityWebApplicationFactory : WebApplicationFactory<Program>
{










    
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder("postgres:16-alpine")
        .WithDatabase("identity_test_db")
        .WithUsername("test_identity_user")
        .WithPassword("test_secure_password")
        .Build();
    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        // 1. Explicitly start the Docker container synchronously/sequentially
        _dbContainer.StartAsync().GetAwaiter().GetResult();

        // 2. Build the underlying web host application
        var host = base.CreateHost(builder);

        // 3. Create a scope to resolve your IdentityDbContext
        using (var scope = host.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            var context = services.GetRequiredService<IdentityDbContext>();

            // 4. Block and apply all pending schema changes before the host starts
            context.Database.MigrateAsync().GetAwaiter().GetResult();
        }

        // 5. Return the fully migrated and ready host to your tests
        return host;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Locate and remove the original DbContext registration
            var descriptor = services.SingleOrDefault(d =>
                d.ServiceType == typeof(DbContextOptions<IdentityDbContext>));

            if (descriptor != null) services.Remove(descriptor);

            // Inject the dynamic connection string provided by the running Testcontainer
            services.AddDbContext<IdentityDbContext>(options =>
                options.UseNpgsql(_dbContainer.GetConnectionString()));


        });
    }


    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
        await _dbContainer.DisposeAsync();
    }
}