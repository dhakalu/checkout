namespace Orders.Contracts;

public record CreateOrderRequest
{
    public Guid CustomerId { get; init; } = default!;

    public ShippingAddress ShippingAddress { get; init; } = default!;

    public List<OrderItem> Items { get; init; } = default!;


};
