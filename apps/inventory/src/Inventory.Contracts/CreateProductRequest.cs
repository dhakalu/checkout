
namespace Inventory.Contracts;

public class CreateProductRequest
{
    public Guid CategoryId { get; init; }
    public Guid BrandId { get; init; }

    public string Name { get; init; } = default!;

    public string Slug { get; init; } = default!;

    public string Description { get; set; } = default!;

}