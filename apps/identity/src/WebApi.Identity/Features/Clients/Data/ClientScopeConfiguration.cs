using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace WebApi.Identity.Features.Clients.Data;

public class ClientScopeConfiguration : IEntityTypeConfiguration<ClientScope>
{
    public void Configure(EntityTypeBuilder<ClientScope> builder)
    {
        builder.ToTable("client_scopes")
            .HasKey(c => new
            {
                c.ClientId,
                c.ScopeKey
            });

        builder.Property(cs => cs.ClientId)
            .HasColumnName("client_id")
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(cs => cs.ScopeKey)
            .HasColumnName("scope_key")
            .HasColumnType("varchar(50)")
            .IsRequired();

        builder.HasOne(cs => cs.Client)
            .WithMany(c => c.Scopes)
            .HasForeignKey(cs => cs.ClientId);

        builder.HasOne(cs => cs.Scope)
            .WithMany()
            .HasForeignKey(cs => cs.ScopeKey);

        builder.HasIndex(x => x.ClientId);
    }
}
