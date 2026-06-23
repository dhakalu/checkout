using Inventory.Domain;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

using Shared.Annotations;

namespace Inventory.Infrastructure.Persistence;

public class ProductsRepository(InventoryDbContext dbContext) : IRepository
{

    public async Task<EntityEntry<Product>> AddAsync(Product product, CancellationToken cancellationToken)
    {
        return await dbContext.AddAsync(product, cancellationToken);
    }

    public async Task<EntityEntry<ProductVariant>> AddVariantAsync(ProductVariant product, CancellationToken token)
    {
        return await dbContext.AddAsync(product, token);
    }

    public async Task<Product?> GetByBrandIdAndNameAsync(Guid brandId, string name, CancellationToken token)
    {
        return await dbContext.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Name == name && p.BrandId == brandId, token);
    }

    public async Task<ProductVariant?> GetVariantByProductIdAndSkuAsync(Guid productId, string sku, CancellationToken token)
    {
        return await dbContext.ProductVariants.AsNoTracking().FirstOrDefaultAsync(p => p.Sku == sku && p.ProductId == productId, token);
    }
}