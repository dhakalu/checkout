namespace Inventory.WebApi.Features.CreateBrand;

public record CreateBrandCommand(
    string Name,
    string Slug,
    string WebsiteUrl,
    string Description,
    bool IsActive
);