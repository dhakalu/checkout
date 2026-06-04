using Microsoft.EntityFrameworkCore;
using System.Dynamic;
using Inventory.Domain;

namespace Inventory.Infrastructure.Persistence;

public class InventorDbContext(DbContextOptions<InventorDbContext> options) : DbContext(options)
{

    public DbSet<Inventor> Inventors { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Scans the current assembly for any IEntityTypeConfiguration classes
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(InventorDbContext).Assembly);
    }
}