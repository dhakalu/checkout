using System;

namespace WebApi.Identity.Features.Scopes.ValidateScopes;

public record ValidateScopesCommand(IReadOnlyCollection<string> Scopes);
