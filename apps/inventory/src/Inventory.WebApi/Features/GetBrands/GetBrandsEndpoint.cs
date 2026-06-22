

using FluentValidation;

using Inventory.Contracts;

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

using Shared.Annotations;

namespace Inventory.WebApi.Features.GetBrands;

public class GetBrandsEndpoint : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("/brands", HandleAsync);
    }

    private async Task<IResult> HandleAsync([AsParameters] GetBrandsQueryParameters queryParameters, GetBrandsHandler handler,
        CancellationToken cancellationToken)
    {
        GetBrandsQuery cmd = new(
            queryParameters.Query,
            queryParameters.Limit,
            queryParameters.Offset
        );
        var results = await handler.HandleAsync(cmd, cancellationToken);
        return Results.Ok(results);
    }
}