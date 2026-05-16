namespace WebApi.Identity.Features.Clients.ManageClientScope;

public record ManageClientScopeCommand(Guid Id, IReadOnlyCollection<string> Scopes);
