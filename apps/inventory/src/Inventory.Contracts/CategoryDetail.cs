namespace Inventory.Contracts;

public record CagetoryDetail(
    Guid Id,
    string Name,
    string Description,
    string Slug,
    bool IsActive
);