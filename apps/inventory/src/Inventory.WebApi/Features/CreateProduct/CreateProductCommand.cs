
namespace Inventory.WebApi.Features.CreateProduct;

public class CreateProductCommand
{
    public Guid BrandId { get; init; }

    public Guid CategoryId { get; init; }

    public string Name { get; init; } = default!;

    public string Slug { get; init; } = default!;

    public string Description { get; set; } = default!;

}