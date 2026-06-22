namespace Inventory.WebApi.Features.GetBrands;

public record GetBrandsQuery(string Query, int Limit, int Offset);