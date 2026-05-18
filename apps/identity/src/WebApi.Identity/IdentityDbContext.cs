namespace WebApi.Identity;

using Microsoft.EntityFrameworkCore;
using WebApi.Identity.Features.Clients.Data;
using WebApi.Identity.Features.Users.Data;

public class IdentityDbContext(DbContextOptions<IdentityDbContext> options) : DbContext(options)
{
    public DbSet<User> Identities { get; set; } = default!;

    public DbSet<Client> Clients { get; set; } = default!;

    public DbSet<Features.Scopes.Data.Scope> Scopes { get; set; } = default!;

    public DbSet<ClientScope> ClientScopes { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Scans the current assembly for any IEntityTypeConfiguration classes
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(Program).Assembly);

        modelBuilder.UseOpenIddict();
    }
}