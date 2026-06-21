namespace Inventory.Domain;

public class Product
{
    public Guid Id { get; init; }

    public Guid CategoryId { get; init; }

    public Guid BrandId { get; init; }

    /// <summary>
    /// The name of the product. This is a required field that provides
    /// a human-readable name for the product. It is used for display purposes
    /// in the user interface and can also be used for searching and
    /// filtering products. The name should be unique within the inventory system
    /// to avoid confusion between different products.
    /// </summary>
    public string Name { get; init; } = default!;

    /// <summary>
    /// A URL-friendly version of the product name, often used in web applications
    /// for routing and SEO purposes.
    /// </summary>
    public string Slug { get; init; } = default!;

    /// <summary>
    /// A brief description of the product. This field provides
    /// additional information about the product and can be used to
    /// help users understand the features and benefits of the
    /// product. The description can be displayed in the user
    /// interface, such as on a product page or in search results,
    /// to provide context and improve the user experience.
    /// </summary>

    public string Description { get; init; } = default!;


    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }

    public Brand Brand { get; init; } = default!;

    public ProductCategory Category { get; init; } = default!;

    public List<ProductVariant> Variants { get; init; } = default!;

}