using System;

namespace WebApi.Identity.Features.Scopes.CreateScope;

public record CreateScopeRequest
{

    public string Key { get; init; } = default!;

    public string Name { get; init; } = default!;

    public string Description { get; init; } = default!;
}
