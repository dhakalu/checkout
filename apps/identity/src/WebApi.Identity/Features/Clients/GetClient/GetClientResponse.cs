using System;

namespace WebApi.Identity.Features.Clients.GetClient;

public record GetClientResponse
{

    public Guid Id { get; init; }

    public string Name { get; init; } = default!;

    public string Description { get; init; } = default!;

    public bool IsActive { get; init; } = default;

}
