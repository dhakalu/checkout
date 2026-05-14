namespace WebApi.Identity;

using Microsoft.EntityFrameworkCore;

public class IdentityDbContext(DbContextOptions<IdentityDbContext> options) : DbContext(options)
{
    public DbSet<Features.Users.Data.Identity> Identities { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Scans the current assembly for any IEntityTypeConfiguration classes
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(Program).Assembly);
    }
}