using Shared.Annotations;
using Orders.Contracts;
using FluentValidation;

namespace Orders.WebApi.Features.Orders.CreateOrder;

public class CreateOrderEndpoint : IEndpoint
{
    public const string Path = "/orders";
    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(Path, HandleAsync)
        .WithName("CreateOrder")
        .WithTags("Orders");
    }

    private async Task<IResult> HandleAsync(CreateOrderRequest req,
        CreateOrderValidator validator,
        CreateOrderHandler handler,
        CancellationToken cancellationToken
        )
    {
        await validator.ValidateAndThrowAsync(req, cancellationToken);
        CreateOrderCommand cmd = new(req.ShippingAddress, req.Items.AsReadOnly());
        await handler.HandleAsync(cmd, cancellationToken);
        return Results.Created();
    }
}