

namespace Orders.Domain;

public class OrderItem(Guid orderId, Guid productId, string productName, string sku, decimal unitPrice, Int32 quantity)
{
    public Guid OrderId { get; private set; } = orderId;
    public Guid ProductId { get; private set; } = productId;
    public string ProductName { get; private set; } = productName;
    public string Sku { get; private set; } = sku;
    public decimal UnitPrice { get; private set; } = unitPrice;
    public int Quantity { get; private set; } = quantity;
    public decimal LineTotal => UnitPrice * Quantity;
}