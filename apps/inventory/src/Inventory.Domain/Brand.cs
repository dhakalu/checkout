namespace Inventory.Domain;

public class Brand
{
    public Guid Id { get; init; }

    public string Name { get; init; } = default!;

    public string Slug { get; init; } = default!;

    public string WebsiteUrl { get; init; } = default!;

    /// <summary>
    /// A brief description of the brand. This field provides
    /// additional information about the brand and can be used to
    /// help users understand the brand's identity and values.
    /// </summary>
    public string Description { get; init; } = default!;

    public bool IsActive { get; init; } = true;

    public ICollection<Product> Products { get; init; } = default!;
}