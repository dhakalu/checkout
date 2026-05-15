namespace WebApi.Identity.Features.Clients;

public record RegisterClientRequest
{
    public string Description {get; init;} = default!;
}