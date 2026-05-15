using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace WebApi.Identity.Features.Clients.Data;

public class ClientSecretConfiguration : IEntityTypeConfiguration<ClientSecret>
{
    public void Configure(EntityTypeBuilder<ClientSecret> builder)
    {
        builder.ToTable("client_secrets");


        builder.Property(p => p.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(p => p.ClientId)
            .HasColumnName("client_id")
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(p => p.Secret)
            .HasColumnName("secret")
            .HasColumnType("varchar(255)")
            .HasMaxLength(255)
            .IsRequired();

        // Index the Foreign Key for rapid join queries during validation
        builder.HasIndex(s => s.ClientId);
    }
}
