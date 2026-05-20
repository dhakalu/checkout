namespace WebApi.Identity.Features.Clients.RegisterClient;

using FluentValidation;
using Shared.Annotations;
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
            IsActive = request.IsActive,
            Description = request.Description,
        };
        var id = await handler.HandleAsync(cmd, cancellationToken);
        return Results.Created($"/clients/{id}", new RegisterClientResponse(id));
    }
}
