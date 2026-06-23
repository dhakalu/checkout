namespace Inventory.Contracts;


public record GetCategoriesQueryParameters(int Limit, int Offset, string? Query = "");