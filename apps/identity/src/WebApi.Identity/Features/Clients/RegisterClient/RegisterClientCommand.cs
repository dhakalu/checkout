namespace WebApi.Identity.Features.Clients.RegisterClient;

public record RegisterClientCommand
{
    public string Name {get; init; } = default!;
    public string Description { get; init; } = default!;

    public bool IsActive {get; init; }
}