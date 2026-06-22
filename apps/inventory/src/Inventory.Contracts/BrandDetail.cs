
namespace Inventory.Contracts;

public record BrandDetail(
    Guid Id,
    string Name,
    string Description,
    string Slug,
    string WebsitUrl,
    bool IsActive
);