using Shared.DependencyInjection;
using WebApi.Identity.Features.Scopes.Data;

namespace WebApi.Identity.Features.Scopes.CreateScope;

public class CreateScopeHandler(ScopeRepository repository) : IHandler
{

    private readonly ScopeRepository _repository = repository;

    public async Task<bool> HandleAsync(CreateScopeCommand cmd, CancellationToken cancellationToken)
    {
        var existingScope = await _repository.GetByKey(cmd.Key, cancellationToken);
        if (existingScope != null)
        {
            throw new InvalidOperationException("Scope with given key already exists.");
        }
        var scope = new Scope
        {
            Key = cmd.Key,
            Name = cmd.Name,
            Description = cmd.Description
        };
        return await _repository.AddAsync(scope, cancellationToken);
    }
}

