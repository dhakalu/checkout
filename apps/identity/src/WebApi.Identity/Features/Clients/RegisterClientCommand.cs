namespace WebApi.Identity.Features.Clients;

public record RegisterClientCommand
{
    public string Description { get; init; } = default!;
}