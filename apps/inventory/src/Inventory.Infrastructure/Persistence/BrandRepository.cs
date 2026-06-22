using System.Collections.Immutable;

using Inventory.Domain;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

using Shared.Annotations;

namespace Inventory.Infrastructure.Persistence;


public class BrandRepository(InventoryDbContext dbContext) : IRepository
{

    public async Task<EntityEntry<Brand>> AddAsync(Brand brand, CancellationToken cancellationToken)
    {
        return await dbContext.AddAsync(brand, cancellationToken);
    }

    public async Task<List<Brand>> GetAllAsync(string query, int limit, int offset, CancellationToken cancellationToken)
    {
        return await dbContext.Brands
        .Where(x =>
            x.Name.Contains(query)
        )
        .OrderBy(b => b.Name)
        .Skip(offset)
        .Take(limit)
        .ToListAsync(cancellationToken);
    }
}