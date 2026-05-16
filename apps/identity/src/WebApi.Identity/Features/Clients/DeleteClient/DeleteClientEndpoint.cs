using Microsoft.AspNetCore.Mvc;

namespace WebApi.Identity.Features.Clients.DeleteClient;

public class DeleteClientEndpoint : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapDelete("/clients/{clientId}", HandleAsync);
    }

    private async Task<IResult> HandleAsync([FromRoute] Guid clientId, DeleteClientHandler handler, CancellationToken cancellationToken)
    {
        await handler.HandleAsync(new DeleteClientCommand(clientId), cancellationToken);
        return Results.NoContent();
    }
}
