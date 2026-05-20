using Microsoft.EntityFrameworkCore;
using System.Dynamic;
using {{cookiecutter.domain_namespace}};

namespace {{cookiecutter.infra_namespace}}.Persistence;

public class {{cookiecutter.__entity_name}}DbContext(DbContextOptions<{{cookiecutter.__entity_name}}DbContext> options) : DbContext(options)
{

    public DbSet<{{cookiecutter.__entity_name}}> {{cookiecutter.__entity_name}}s { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Scans the current assembly for any IEntityTypeConfiguration classes
        modelBuilder.ApplyConfigurationsFromAssembly(typeof({{cookiecutter.__entity_name}}DbContext).Assembly);
    }
}