namespace Inventory.Domain;

public class ProductCategory
{
    /// <summary>
    /// The unique identifier for the product category. 
    /// This is a GUID that is generated when a new 
    /// product category is created. It serves as the 
    /// primary key for the product category in the database 
    /// and is used to reference the category in other parts 
    /// of the application.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// The name of the product category. This is a required
    /// field that provides a human-readable name for the
    /// category. It is used for display purposes in the user
    /// interface and can also be used for searching and
    /// filtering products by category. The name should be
    /// unique within the inventory system to avoid confusion
    /// between different categories.
    /// </summary>
    public string Name { get; init; } = default!;

    /// <summary>
    /// A URL-friendly version of the category name, 
    /// often used in web applications for routing and 
    /// SEO purposes.
    /// </summary>
    public string Slug { get; init; } = default!;

    /// <summary>
    /// A brief description of the product category. This
    /// field provides additional information about the category
    /// and can be used to help users understand what types of
    /// products are included in the category. The description
    /// can be displayed in the user interface, such as on a
    /// category page or in search results, to provide context
    /// and improve the user experience.
    /// </summary>
    public string Description { get; init; } = default!;

    public bool IsActive { get; init; } = true;

    public List<Product> Products { get; init; } = default!;
}