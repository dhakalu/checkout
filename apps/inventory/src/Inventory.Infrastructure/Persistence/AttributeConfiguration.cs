using Inventory.Domain;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory.Infrastructure.Persistence;

public class AttributeConfiguration : IEntityTypeConfiguration<Domain.Attribute>
{
    public void Configure(EntityTypeBuilder<Domain.Attribute> builder)
    {
        builder.ToTable("attributes");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasColumnType("varchar(500)")
            .IsRequired();

        builder.Property(x => x.Type)
            .HasColumnName("type")
            .HasColumnType("varchar(50)")
            .IsRequired();
    }
}