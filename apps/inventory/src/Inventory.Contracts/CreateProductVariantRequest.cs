namespace Inventory.Contracts;

public record CreateProductVariantRequest(
    string Sku,
    string Name,
    decimal Price,
    decimal Cost,
    decimal ComparePrice
);