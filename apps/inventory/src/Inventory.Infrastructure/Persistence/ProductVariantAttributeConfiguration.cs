using Inventory.Domain;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory.Infrastructure.Persistence;

public class ProductVariantAttributeConfiguration : IEntityTypeConfiguration<ProductVariantAttribute>
{
    public void Configure(EntityTypeBuilder<ProductVariantAttribute> builder)
    {
        builder.ToTable("product_variant_attributes");

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(x => x.ProductVariantId)
            .HasColumnName("product_variant_id")
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(x => x.AttributeId)
            .HasColumnName("attribute_id")
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(x => x.Value)
            .HasColumnName("value")
            .HasColumnType("text")
            .IsRequired();
    }
}