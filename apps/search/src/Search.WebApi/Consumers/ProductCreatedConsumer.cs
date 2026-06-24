using Inventory.Contracts.Events;

using MassTransit;

namespace Search.WebApi.Consumers;

public class ProductCreatedConsumer(ILogger<ProductCreatedConsumer> logger) : IConsumer<ProductCreated>
{
    public async Task Consume(ConsumeContext<ProductCreated> context)
    {
        logger.LogInformation("consumed product {}", context.Message.ProductId);
    }
}