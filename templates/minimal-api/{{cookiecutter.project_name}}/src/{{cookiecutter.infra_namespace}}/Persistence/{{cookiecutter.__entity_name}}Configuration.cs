using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using {{cookiecutter.domain_namespace}};

namespace {{cookiecutter.infra_namespace}}.Persistence;

public class {{cookiecutter.__entity_name}}Configuration : IEntityTypeConfiguration<{{cookiecutter.__entity_name}}>
{
    public void Configure(EntityTypeBuilder<{{cookiecutter.__entity_name}}> builder)
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
