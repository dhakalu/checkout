using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace WebApi.Identity.Features.Users.Data;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id)
            .HasColumnName("id")
            .HasColumnType("varchar(36)")
            .IsRequired();
        builder.Property(i => i.Email)
            .HasColumnName("email")
            .HasColumnType("varchar(255)")
            .IsRequired();
        builder.Property(i => i.IsEmailVerified)
            .HasColumnName("is_email_verified")
            .HasColumnType("boolean")
            .HasDefaultValue(false);
        
        builder.Property(i => i.IsLocked)
            .HasColumnName("is_locked")
            .HasColumnType("boolean")
            .HasDefaultValue(false);

        builder.Property(i => i.IsMfaEnabled)
            .HasColumnName("is_mfa_endabled")
            .HasColumnType("boolean")
            .HasDefaultValue(false);

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
        builder.Property(i => i.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("now()");
        
        builder.Property(i => i.CreatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("now()");   
        }
}
