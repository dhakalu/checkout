using Inventory.Domain;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory.Infrastructure.Persistence;

public class ProductsInventoryConfiguration : IEntityTypeConfiguration<ProductsInventory>
{
    public void Configure(EntityTypeBuilder<ProductsInventory> builder)
    {
        builder.ToTable("products_inventory");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(x => x.ProductVariantId)
            .HasColumnName("product_variant_id")
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(x => x.QuantityOnHand)
            .HasColumnName("quantity_on_hand")
            .HasColumnType("int")
            .IsRequired();

        builder.Property(x => x.QuantityReserved)
            .HasColumnName("quantity_reserved")
            .HasColumnType("int")
            .IsRequired();

        builder.Property(x => x.ReorderPoint)
            .HasColumnName("reorder_point")
            .HasColumnType("int")
            .IsRequired();

        builder.Property(x => x.ReorderQuantity)
            .HasColumnName("reorder_quantity")
            .HasColumnType("int")
            .IsRequired();

        builder.Property(x => x.LastUpdated)
            .HasColumnName("updated_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.HasOne(x => x.ProductVariant)
            .WithOne()
            .HasForeignKey<ProductsInventory>(x => x.ProductVariantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}