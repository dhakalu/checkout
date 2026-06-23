namespace Inventory.Contracts;

public record GetCategoriesResponse(int TotalCount, List<CagetoryDetail> Data);