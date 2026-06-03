using Fulfillment.Workflows;

using MassTransit;

using Microsoft.Extensions.Logging;

using Orders.Contracts.Events;

using Temporalio.Client;

namespace Fulfillment.Orchrestrator.Consumers;

public class OrderCreatedConsumer(ITemporalClient client, ILogger<OrderCreatedConsumer> logger) : IConsumer<OrderCreated>
{
    public async Task Consume(ConsumeContext<OrderCreated> context)
    {
        var request = new FulfillmentWorkflowRequest(
            context.Message.OrderId,
            context.Message.CustomerId,
            context.Message.ShippingAddress,
            context.Message.Items
        );


        var result = await client.ExecuteWorkflowAsync<FulfillmentWorkflow, string>(
            (wf) => wf.StartFulfillmentAsync(request),
            new()
            {
                Id = Guid.NewGuid().ToString(),
                TaskQueue = "checkout-order-fulfillment"
            }
        );
        logger.LogInformation("Fulfillment workflow result: {Result}", result);
    }
}