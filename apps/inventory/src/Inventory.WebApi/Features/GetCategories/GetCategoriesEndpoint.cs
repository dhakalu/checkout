using Inventory.Contracts;

using Shared.Annotations;

namespace Inventory.WebApi.Features.GetCategories;


public class GetCategoriesEndpoint : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {

        app.MapGet("/categories", HandleAsync);
    }

    private async Task<IResult> HandleAsync([AsParameters] GetCategoriesQueryParameters queryParameters,
            GetCategoriesHandler handler,
            CancellationToken cancellationToken
        )
    {

        var result = await handler.HandleAsync(new(queryParameters.Limit, queryParameters.Offset, queryParameters.Query), cancellationToken);
        return Results.Ok(result);
    }
}