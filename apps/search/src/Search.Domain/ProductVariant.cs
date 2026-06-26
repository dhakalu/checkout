namespace Search.Domain;


public class ProductVariant
{
    public Guid Id { get; set; }

    public string Name { get; init; } = default!;

    public string Sku { get; init; } = default!;

    public decimal Price { get; init; }

    public decimal ComparePrice { get; init; }

    public bool IsInStock { get; init; }

    public string Size { get; init; } = default!;

    public string Color { get; init; } = default!;

    public string Theme { get; init; } = default!;

}