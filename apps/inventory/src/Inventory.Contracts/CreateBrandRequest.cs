
namespace Inventory.Contracts;

public record CreateBrandRequest(
    string Name,
    string Slug,
    string WebsiteUrl,
    string Description,
    bool IsActive
);