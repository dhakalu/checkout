namespace Inventory.Domain;

public class Attribute
{

    public Guid Id { get; init; }

    public string Name { get; init; } = default!;

    /// <summary>
    /// The type of the attribute, which defines the kind of data it holds.
    ///  This is a required field that specifies the data type of the 
    /// attribute's value, such as string, integer, decimal, boolean, etc. The type is important for validating the attribute's value and ensuring that it is stored and processed correctly in the inventory system. It can also be used to determine how the attribute is displayed in the user interface and how it can be used for filtering and searching products.
    /// </summary>
    public string Type { get; init; } = default!;

}