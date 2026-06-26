using FluentValidation;

using Inventory.Contracts;

using Shared.Annotations;

namespace Inventory.WebApi.Features.CraeteCategory;

public class CreateCategoryEndpoint : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost("categories", HandleAsync);
    }

    private async Task<IResult> HandleAsync(CreateCategoryRequest request, CreateCategoryValidator validator, CreateCategoryHandler handler, CancellationToken cancellation)
    {
        await validator.ValidateAndThrowAsync(request, cancellation);
        var category = await handler.HandleAsync(new(request.Name, request.Description, request.Slug, request.IsActive), cancellation);
        return Results.Created($"/categories/{category.Id}", category);
    }

}