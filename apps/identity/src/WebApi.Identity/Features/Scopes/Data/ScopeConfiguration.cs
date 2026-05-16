using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace WebApi.Identity.Features.Scopes.Data;

public class ScopeConfiguration : IEntityTypeConfiguration<Scope>
{
    public void Configure(EntityTypeBuilder<Scope> builder)
    {
        builder.ToTable("scopes")
            .HasKey(c => c.Key);

        #region columns
        builder.Property(s => s.Key)
            .HasColumnName("key")
            .HasColumnType("varchar(50)")
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
        #endregion
        # region index
        builder.HasIndex(c => c.Key).IsUnique();
        #endregion
    }
}
