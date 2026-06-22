

using Inventory.Domain;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

using Shared.Annotations;

namespace Inventory.Infrastructure.Persistence;

public class ProductCategoryRepository(InventoryDbContext dbContext) : IRepository
{


    public async Task<EntityEntry<ProductCategory>> AddAsync(ProductCategory category, CancellationToken cancellation)
    {
        return await dbContext.AddAsync(category, cancellation);
    }

    public async Task<List<ProductCategory>> GetAllAsync(int limit, int offset, string? query, CancellationToken ct)
    {
        return await dbContext.Categories
            .AsNoTracking()
            .Skip(offset)
            .Take(limit)
            .ToListAsync(ct);
    }
}