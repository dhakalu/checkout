using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Orders.Domain;

namespace Orders.Infrastructure.Persistence;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders");

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasColumnType("varchar(50)")
            .HasMaxLength(50)
            .HasConversion<string>();

        builder.OwnsOne(x => x.ShippingAddress, sa =>
        {
            sa.Property(x => x.Street).HasColumnName("shipping_street").IsRequired();
            sa.Property(x => x.City).HasColumnName("shipping_city").IsRequired();
            sa.Property(x => x.State).HasColumnName("shipping_state").IsRequired();
            sa.Property(x => x.ZipCode).HasColumnName("shipping_zip_code").IsRequired();
        });

        builder.HasMany(x => x.OrderItems)
            .WithOne()
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

    }
}