using FluentValidation;
using FluentValidation.Results;
using Shared.DependencyInjection;
using WebApi.Identity.Features.Clients.Data;
using WebApi.Identity.Features.Scopes.ValidateScopes;

namespace WebApi.Identity.Features.Clients.ManageClientScope;

public class ManageClientScopeHandler(ValidateScopesHandler validateScopesHandler, ClientRepository clientRepository) : IHandler
{

    private readonly ValidateScopesHandler _validateScopesHandler = validateScopesHandler;

    private readonly ClientRepository _clientRepository = clientRepository;

    public async Task<bool> HandleAsync(ManageClientScopeCommand cmd, CancellationToken cancellationToken)
    {
        var scopeValidation = await _validateScopesHandler.HandleAsync(new ValidateScopesCommand(cmd.Scopes), cancellationToken);

        if (scopeValidation.Invalid.Count > 0)
        {
            var failedScopes = string.Join(", ", scopeValidation.Invalid);
            var failure = new ValidationFailure("scopes", $"These scopes are invalid: {failedScopes}.");
            throw new ValidationException("Invalid scopes.", [failure]);
        }

        return await _clientRepository.UpdateScopesAsync(cmd.Id, cmd.Scopes, cancellationToken);
    }

}
