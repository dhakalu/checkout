using Inventory.Contracts.Events;

using MassTransit;

using Search.Domain;
using Search.WebApi.Features.IndexProducts;

namespace Search.WebApi.Consumers;

public class ProductCreatedConsumer(
    ILogger<ProductCreatedConsumer> logger,
    IndexProductsHandler handler
) : IConsumer<ProductCreated>
{
    public async Task Consume(ConsumeContext<ProductCreated> context)
    {
        var cts = new CancellationTokenSource();
        var ct = cts.Token;
        var message = context.Message;
        ProductDocument productDocument = new()
        {
            Id = message.ProductId,
            BrandId = message.BrandId,
            BrandName = message.BrandName,
            CategoryId = message.CategoryId,
            CategoryName = message.CategoryName,
            Title = message.Name,
            Description = message.Description
        };
        var isSuccess = await handler.HandleAsync(productDocument, ct);
        if (!isSuccess)
        {
            throw new Exception("cannot save the product document to index");
        }
    }
}