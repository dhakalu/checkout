namespace Inventory.Domain;

public class ProductVariant
{
    public Guid Id { get; init; }

    public Guid ProductId { get; init; }

    /// <summary>
    /// The SKU (Stock Keeping Unit) of the product variant. 
    /// This is a unique identifier for the specific variant
    ///  of the product, such as a particular size or color. 
    /// The SKU is used for inventory management and tracking 
    /// purposes, allowing businesses to differentiate between
    ///  different variants of the same product. It should be 
    /// unique across all product variants in the inventory 
    /// system to avoid confusion and ensure accurate inventory 
    /// tracking.
    /// </summary>
    public string Sku { get; init; } = default!;

    /// <summary>

    /// <summary>
    /// The name of the product. This is a required field that provides
    /// a human-readable name for the product. It is used for display purposes
    /// in the user interface and can also be used for searching and
    /// filtering products. The name should be unique within the inventory system
    /// to avoid confusion between different products.
    /// </summary>
    public string Name { get; init; } = default!;


    /// <summary>
    /// The price of the product. This is a required field that represents
    /// the cost of the product to the customer. The price should be a positive
    /// decimal value and can be used for calculating totals in the shopping
    /// cart and during the checkout process. It is important to ensure that the
    /// price is accurate and up-to-date to avoid issues with customer orders and
    /// inventory management.
    /// </summary>
    public decimal Price { get; init; }

    /// <summary>
    /// The cost of the product. This field represents the cost of the product to the
    /// business. It is used for calculating profit margins and can be important for 
    /// inventory management and financial reporting. The cost should be a positive 
    /// decimal value and should be kept accurate to ensure proper financial tracking 
    /// and decision-making within the business.
    /// </summary>
    public decimal Cost { get; init; }

    /// <summary>
    /// The compare price of the product. 
    /// This field represents the original price of the product before 
    /// any discounts or promotions are applied. It is used for displaying 
    /// the savings to the customer and can be important for marketing and sales analysis.
    /// </summary>
    public decimal ComparePrice { get; init; }

    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }

    public Product Product { get; init; } = default!;

}