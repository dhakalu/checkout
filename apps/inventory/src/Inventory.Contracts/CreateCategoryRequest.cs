namespace Inventory.Contracts;

public record CreateCategoryRequest(string Name, string Description, string Slug, bool IsActive);