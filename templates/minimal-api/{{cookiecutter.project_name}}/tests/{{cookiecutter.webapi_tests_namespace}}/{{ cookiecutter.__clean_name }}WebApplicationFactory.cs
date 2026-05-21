using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.PostgreSql;
using Microsoft.EntityFrameworkCore;

using {{cookiecutter.webapi_namespace}};
using {{cookiecutter.infra_namespace}}.Persistence;

namespace {{cookiecutter.webapi_tests_namespace}};

public class {{cookiecutter.__clean_name}}WebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder("postgres:16-alpine")
        .WithDatabase("{{cookiecutter.project_name}}_test_db")
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
            var context = services.GetRequiredService<{{cookiecutter.__entity_name}}DbContext>();

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
                d.ServiceType == typeof(DbContextOptions<{{cookiecutter.__entity_name}}DbContext>));

            if (descriptor != null) services.Remove(descriptor);

            // Inject the dynamic connection string provided by the running Testcontainer
            services.AddDbContext<{{cookiecutter.__entity_name}}DbContext>(options =>
                options.UseNpgsql(_dbContainer.GetConnectionString()));


        });
    }


    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        await _dbContainer.DisposeAsync();
    }
}