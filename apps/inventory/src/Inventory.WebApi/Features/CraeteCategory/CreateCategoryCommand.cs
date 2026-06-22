namespace Inventory.WebApi.Features.CraeteCategory;

public record CreateCategoryCommand(string Name, string Description, string Slug, bool IsActive);