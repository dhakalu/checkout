using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace WebApi.Identity.Features.Signup.Data;

public class IdentityConfiguration : IEntityTypeConfiguration<Identity>
{
    public void Configure(EntityTypeBuilder<Identity> builder)
    {
        builder.ToTable("identities");

        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id)
            .HasColumnName("id")
            .HasColumnType("varchar(36)")
            .IsRequired();
        builder.Property(i => i.Email)
            .HasColumnName("email")
            .HasColumnType("varchar(255)")
            .IsRequired();
        builder.Property(i => i.PasswordHash)
            .HasColumnName("password_hash")
            .HasColumnType("varchar(255)")
            .IsRequired();
        builder.Property(i => i.FirstName)
            .HasColumnName("first_name")
            .HasColumnType("varchar(255)")
            .IsRequired();
        builder.Property(i => i.LastName)
            .HasColumnName("last_name")
            .HasColumnType("varchar(255)")
            .IsRequired();
    }
}
