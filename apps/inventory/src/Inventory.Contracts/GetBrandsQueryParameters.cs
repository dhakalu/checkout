namespace Inventory.Contracts;

public record GetBrandsQueryParameters(
    string Query,
    int Limit,
    int Offset
);