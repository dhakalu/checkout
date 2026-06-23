
namespace Inventory.WebApi.Features.CreateProductVariant;

public record CreateProductVariantCommand(
    Guid ProductId,
    string Sku,
    string Name,
    decimal Price,
    decimal Cost,
    decimal ComparePrice
);