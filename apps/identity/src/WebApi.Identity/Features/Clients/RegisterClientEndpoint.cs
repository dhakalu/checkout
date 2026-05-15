namespace WebApi.Identity.Features.Clients;

using System.ComponentModel;
using FluentValidation;
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
        RegisterClientValidator validator,
        RegisterClientHandler handler,
        CancellationToken cancellationToken
    )
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);
        var cmd = new RegisterClientCommand
        {
            Name = request.Name,
            Description = request.Description
        };
        var isCreated = await handler.HandleAsync(cmd, cancellationToken);
        if (isCreated)
        {
            return Results.Created();
        }
        throw new Exception("Cannot create client at this time.");
    }
}
