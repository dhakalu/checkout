using System.Dynamic;

using Inventory.Domain;

using Microsoft.EntityFrameworkCore;

using Shared.Annotations;

namespace Inventory.Infrastructure.Persistence;

public class InventoryDbContext(DbContextOptions<InventoryDbContext> options) : DbContext(options), IUnitOfWork
{

    public DbSet<ProductsInventory> Inventories { get; set; }


    public DbSet<Brand> Brands { get; set; }
    public DbSet<ProductCategory> Categories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductVariant> ProductVariants { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Scans the current assembly for any IEntityTypeConfiguration classes
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(InventoryDbContext).Assembly);
    }
}