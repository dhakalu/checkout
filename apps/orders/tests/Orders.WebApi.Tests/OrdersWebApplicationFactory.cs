using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.PostgreSql;
using Microsoft.EntityFrameworkCore;

using Orders.WebApi;
using Orders.Infrastructure.Persistence;

namespace Orders.WebApi.Tests;

public class OrdersWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder("postgres:16-alpine")
        .WithDatabase("orders_test_db")
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
            var context = services.GetRequiredService<OrderDbContext>();

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
                d.ServiceType == typeof(DbContextOptions<OrderDbContext>));

            if (descriptor != null) services.Remove(descriptor);

            // Inject the dynamic connection string provided by the running Testcontainer
            services.AddDbContext<OrderDbContext>(options =>
                options.UseNpgsql(_dbContainer.GetConnectionString()));


        });
    }


    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        await _dbContainer.DisposeAsync();
    }
}