namespace WebApi.Identity.Tests;

using Testcontainers.PostgreSql;
using Microsoft.EntityFrameworkCore;
using WebApi.Identity;
using Xunit;

public class DatabaseFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder("postgres:15-alpine")
        .WithDatabase("central_test_db")
        .WithUsername("central_user")
        .WithPassword("central_password")
        .Build();
    public IdentityDbContext DbContext { get; private set; } = default!;

    public async ValueTask InitializeAsync()
    {
        await _postgresContainer.StartAsync();
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseNpgsql(_postgresContainer.GetConnectionString())
            .Options;

        DbContext = new IdentityDbContext(options);
        await DbContext.Database.EnsureCreatedAsync();
        using var context = new IdentityDbContext(options);
        await context.Database.MigrateAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await _postgresContainer.DisposeAsync();
    }
}