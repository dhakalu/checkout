

using FluentValidation;

using Inventory.Contracts;

using Microsoft.AspNetCore.Components.Routing;

using Shared.Annotations;

namespace Inventory.WebApi.Features.CreateProduct;

public class ProductEndpoints : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder group)
    {

        group.MapPost("/products", HandleCreateProductAsync);

    }

    private async Task<IResult> HandleCreateProductAsync(CreateProductRequest request, CreateProductValidator vaidator, CreateProductHandler handler, CancellationToken token)
    {
        await vaidator.ValidateAndThrowAsync(request, token);
        var cmd = new CreateProductCommand()
        {
            BrandId = request.BrandId,
            CategoryId = request.CategoryId,
            Name = request.Name,
            Description = request.Description,
            Slug = request.Slug
        };
        await handler.HandleAsync(cmd, token);
        return Results.Created();
    }
}