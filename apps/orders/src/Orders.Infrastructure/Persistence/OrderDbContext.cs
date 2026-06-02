using MassTransit;

using Microsoft.EntityFrameworkCore;

using Orders.Domain;

namespace Orders.Infrastructure.Persistence;

public class OrderDbContext(DbContextOptions<OrderDbContext> options) : DbContext(options)
{

    public DbSet<Order> Orders { get; set; }

    public DbSet<OrderItem> OrderItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Scans the current assembly for any IEntityTypeConfiguration classes
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrderDbContext).Assembly);

        modelBuilder.AddInboxStateEntity((c) => c.ToTable("inbox_state"));
        modelBuilder.AddOutboxMessageEntity((c) => c.ToTable("outbox_message"));
        modelBuilder.AddOutboxStateEntity((c) => c.ToTable("outbox_state"));
    }
}