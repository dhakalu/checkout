namespace Orders.Domain;

public class Order
{
    public Guid Id { get; } = Guid.NewGuid();

    public OrderStatus Status { get; set; } = default!;

    public ShippingAddress ShippingAddress { get; init; } = default!;

    private readonly List<OrderItem> _orderItems = [];

    public IReadOnlyList<OrderItem> OrderItems => _orderItems;

    private Order() { }

    public Order(IEnumerable<OrderItem> orderItems, OrderStatus status, ShippingAddress shippingAddress)
    {
        ShippingAddress = shippingAddress;
        Status = status;
        _orderItems.AddRange(orderItems);
    }
}