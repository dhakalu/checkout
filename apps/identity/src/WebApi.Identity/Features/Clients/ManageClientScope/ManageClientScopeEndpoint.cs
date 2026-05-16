using Microsoft.AspNetCore.Mvc;

namespace WebApi.Identity.Features.Clients.ManageClientScope;

public class ManageClientScopeEndpoint : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPut("/clients/{clientId}/scopes", HandleAsync);
    }

    private async Task<IResult> HandleAsync([FromRoute]Guid clientId, ManageClientScopeRequest request, ManageClientScopeHandler handler, CancellationToken token)
    {
        await handler.HandleAsync(new ManageClientScopeCommand(clientId, request.Scopes), token);
        return Results.NoContent();
    }
}
