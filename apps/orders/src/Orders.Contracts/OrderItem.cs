namespace Orders.Contracts;

public record OrderItem
{

    /// <summary>
    /// The unique identifier of the product being ordered. 
    /// This is crucial for identifying which product the 
    /// customer wants to purchase and for inventory management purposes.
    /// </summary>
    public Guid ProductId { get; init; } = default!;

    public string ProductName {get; init; } = default!;

    public string Sku {get; init;} = default!;

    /// <summary>
    /// The quantity of the product being ordered. This should be a positive integer and is essential for calculating the total cost of the order.
    /// </summary>
    public int Quantity { get; init; }

    /// <summary>
    /// The price of a single unit of the product at the time of order creation.
    /// This is important to capture to ensure that the order total remains consistent even if product prices change in the future.
    /// </summary>
    public decimal UnitPrice { get; init; }
}