using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Inventory.Domain;

namespace Inventory.Infrastructure.Persistence;

public class InventorConfiguration : IEntityTypeConfiguration<Inventor>
{
    public void Configure(EntityTypeBuilder<Inventor> builder)
    {
        builder.ToTable("example");

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasColumnType("varchar(100)")
            .IsRequired();
    }
}
