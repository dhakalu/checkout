using System;

namespace WebApi.Identity.Features.Clients.ManageClientScope;

public record ManageClientScopeRequest(IReadOnlyCollection<string> Scopes);
