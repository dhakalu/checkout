using System;
using WebApi.Identity.Features.Scopes.Data;

namespace WebApi.Identity.Features.Scopes.ValidateScopes;

public class ValidateScopesHandler(ScopeRepository repository): IHandler
{

    private readonly ScopeRepository _repository = repository;
    public async Task<ValidateScopesResponse> HandleAsync(ValidateScopesCommand cmd, CancellationToken cancellationToken)
    {
        
        List<Scope> scopes = await _repository.ExistsAsync(cmd.Scopes, cancellationToken);
        List<string> validScopes = [.. scopes.Select(s => s.Key)];
        
        if (validScopes.Count == cmd.Scopes.Count)
        {
            return new ValidateScopesResponse
            {
                Valid = validScopes,
                Invalid = []
            };
        }

        var invalidScopes = cmd.Scopes.Where(s => !validScopes.Contains(s)).ToList();

        return new ValidateScopesResponse
        {
            Valid = validScopes,
            Invalid = invalidScopes
        };
    }
}
