using Inventory.Infrastructure.Persistence;
using Inventory.WebApi;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;

using Testcontainers.PostgreSql;

namespace Inventory.WebApi.Tests;

public class InventoryWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder("postgres:16-alpine")
        .WithDatabase("inventory_test_db")
        .WithUsername("test_identity_user")
        .WithPassword("test_secure_password")
        .Build();
    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        // GetAwaiter().GetResult() is intentional — CreateHost has no async overload
        _dbContainer.StartAsync().GetAwaiter().GetResult();

        var host = base.CreateHost(builder);

        using (var scope = host.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            var context = services.GetRequiredService<InventoryDbContext>();

            context.Database.MigrateAsync().GetAwaiter().GetResult();
        }

        return host;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Locate and remove the original DbContext registration
            var descriptor = services.SingleOrDefault(d =>
                d.ServiceType == typeof(DbContextOptions<InventoryDbContext>));

            if (descriptor != null) services.Remove(descriptor);

            // Inject the dynamic connection string provided by the running Testcontainer
            services.AddDbContext<InventoryDbContext>(options =>
                options.UseNpgsql(_dbContainer.GetConnectionString()));
        });
    }


    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        await _dbContainer.DisposeAsync();
    }
}