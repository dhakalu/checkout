using Microsoft.AspNetCore.Mvc;

namespace WebApi.Identity.Features.Clients.GetClient;

public class GetClientEndpoint : IEndpoint
{
   
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("/clients" + "/{id}", HandleAsync);
    }

    private static async Task<GetClientResponse> HandleAsync([FromRoute]Guid id, GetClientHandler handler, CancellationToken cancellationToken)
    {
        var query = new GetClientQuery(id);
        return await handler.HandleAsync(query, cancellationToken);
    }
}
