namespace Inventory.Domain;

public class ProductsInventory
{
    public Guid Id { get; init; }

    public Guid ProductVariantId { get; init; }

    public int QuantityOnHand { get; init; }

    public int QuantityReserved { get; init; }

    public int ReorderPoint { get; init; }

    public int ReorderQuantity { get; init; }

    public DateTime LastUpdated { get; init; }

    public ProductVariant ProductVariant { get; init; } = default!;
}