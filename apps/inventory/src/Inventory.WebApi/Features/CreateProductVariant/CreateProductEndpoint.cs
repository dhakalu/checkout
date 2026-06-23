

using FluentValidation;

using Inventory.Contracts;

using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Mvc;

using Shared.Annotations;

namespace Inventory.WebApi.Features.CreateProductVariant;

public class CreateProductVariantEndpoint : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder group)
    {

        group.MapPost("/products/{productId}/variants", HandleAsync);

    }

    private async Task<IResult> HandleAsync(
        [FromRoute] Guid productId,
        CreateProductVariantRequest request,
        CreateProductVariantValidator vaidator,
        CreateProductVariantHandler handler,
        CancellationToken token)
    {
        await vaidator.ValidateAndThrowAsync(request, token);
        var cmd = new CreateProductVariantCommand(productId, request.Sku, request.Name, request.Price, request.Cost, request.ComparePrice);
        await handler.HandleAsync(cmd, token);
        return Results.Created();
    }
}