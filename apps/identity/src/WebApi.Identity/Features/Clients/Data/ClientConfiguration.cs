using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace WebApi.Identity.Features.Clients.Data;

public class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.ToTable("clients")
            .HasKey(c => c.Id);

        #region columns
        builder.Property(c => c.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .HasDefaultValueSql("gen_random_uuid()");
        
        builder.Property(c => c.Name)
            .HasColumnName("name")
            .HasColumnType("varchar(100)")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(c => c.Description)
            .HasColumnName("description")
            .HasColumnType("varchar(500)")
            .HasMaxLength(500)
            .IsRequired();
        
        builder.Property(c => c.IsActive)
            .HasColumnName("is_active")
            .HasColumnType("boolean")
            .HasDefaultValue(false);
        #endregion

        #region  relations
        builder.HasMany(c => c.Secrets)
            .WithOne()
            .HasForeignKey(s => s.ClientId)
            .OnDelete(DeleteBehavior.Cascade);
        #endregion

        #region index

        builder.HasIndex(c => c.Id)
               .IsUnique();
        #endregion

    }
}