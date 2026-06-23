namespace Inventory.WebApi.Features.GetCategories;

public record GetCategoriesQuery(int Limit, int Offset, string? Query = "");