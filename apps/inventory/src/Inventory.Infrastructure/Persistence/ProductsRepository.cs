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

    public async Task<Product?> GetByBrandIdAndNameAsync(Guid brandId, string name, CancellationToken token)
    {
        return await dbContext.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Name == name && p.BrandId == brandId, token);
    }
}