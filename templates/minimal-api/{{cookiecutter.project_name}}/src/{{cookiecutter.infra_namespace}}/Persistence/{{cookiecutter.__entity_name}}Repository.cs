using Microsoft.EntityFrameworkCore;
using Shared.Annotations;
using {{cookiecutter.domain_namespace}};

namespace {{cookiecutter.infra_namespace}}.Persistence;


public class {{cookiecutter.__entity_name}}Repository({{cookiecutter.__entity_name}}DbContext dbContext) : IRepository
{
    private readonly {{cookiecutter.__entity_name}}DbContext _dbContext = dbContext;


    public async Task AddAsync({{cookiecutter.__entity_name}} identity)
    {
        _dbContext.{{cookiecutter.__entity_name}}s.Add(identity);
        await _dbContext.SaveChangesAsync();
    }
}