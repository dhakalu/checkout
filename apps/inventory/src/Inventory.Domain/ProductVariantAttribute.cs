namespace Inventory.Domain;

public class ProductVariantAttribute
{
    public Guid Id { get; init; }

    public Guid ProductVariantId { get; init; }

    public Guid AttributeId { get; init; }

    public string Value { get; init; } = default!;
}