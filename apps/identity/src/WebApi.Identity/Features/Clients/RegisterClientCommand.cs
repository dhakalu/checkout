namespace WebApi.Identity.Features.Clients;

public record RegisterClientCommand
{
    public string Name {get; init; } = default!;
    public string Description { get; init; } = default!;

    public bool IsActive {get; init; }
}