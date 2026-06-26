using Microsoft.VisualBasic;

namespace Search.Domain;

public class ProductDocument
{

    public Guid Id { get; init; }

    public string Title { get; init; } = default!;
    public string Description { get; init; } = default!;
    public Guid BrandId { get; init; } = default!;
    public string BrandName { get; init; } = default!;
    public Guid CategoryId { get; init; }
    public string CategoryName { get; init; } = default!;

    public List<ProductVariant> Varinats { get; init; } = [];


}