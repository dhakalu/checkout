namespace WebApi.Identity.Features.Clients;

using System.ComponentModel;
using Microsoft.AspNetCore.Http.HttpResults;
using WebApi.Identity;

public class RegisterClientEndpoint : IEndpoint
{
    public const string Path = "/clients";
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost(Path, HandleAsync);
    }

    private async Task<IResult> HandleAsync(
        RegisterClientRequest request,
        RegisterClientHandler handler)
    {
        var cmd = new RegisterClientCommand
        {
            Description = request.Description
        };
        var isCreated = await handler.HandleAsync(cmd);
        if (isCreated)
        {
            return Results.Created();
        }
        throw new Exception("Cannot create client at this time.");
    }
}
