

using Inventory.Domain;

using Microsoft.EntityFrameworkCore.ChangeTracking;

using Shared.Annotations;

namespace Inventory.Infrastructure.Persistence;

public class ProductCategoryRepository(InventoryDbContext dbContext) : IRepository
{


    public async Task<EntityEntry<ProductCategory>> AddAsync(ProductCategory category, CancellationToken cancellation)
    {
        return await dbContext.AddAsync(category, cancellation);
    }
}