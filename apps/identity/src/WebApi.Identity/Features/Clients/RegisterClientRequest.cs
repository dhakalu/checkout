namespace WebApi.Identity.Features.Clients;

public record RegisterClientRequest
{
    public string Description {get; init;} = default!;

    public string Name {get; init; } = default!;

    public bool IsActive {get; set; } = false;

}