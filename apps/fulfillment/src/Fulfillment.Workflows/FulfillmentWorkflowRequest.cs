
using Orders.Contracts;

namespace Fulfillment.Workflows;

public record FulfillmentWorkflowRequest(
    Guid OrderId,
    Guid CustomerId,
    ShippingAddress ShippingAddress,
    IReadOnlyCollection<OrderItem> Items
);