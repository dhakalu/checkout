

using FluentValidation;

using Inventory.Contracts;

using Microsoft.AspNetCore.Http.HttpResults;

using Shared.Annotations;

namespace Inventory.WebApi.Features.CreateBrand;

public class CreateBrandEndpoint : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost("/brands", HandleAsync);
    }

    private async Task<IResult> HandleAsync(CreateBrandRequest request,
        CreateBrandValidator validator, CreateBrandHandler handler,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);
        CreateBrandCommand cmd = new(
            request.Name,
            request.Slug,
            request.WebsiteUrl,
            request.Description,
            request.IsActive
        );
        var isCreated = await handler.HandleAsync(cmd, cancellationToken);
        return isCreated ? Results.Created() : Results.InternalServerError();
    }
}