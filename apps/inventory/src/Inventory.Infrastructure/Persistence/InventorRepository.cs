using Microsoft.EntityFrameworkCore;
using Shared.Annotations;
using Inventory.Domain;

namespace Inventory.Infrastructure.Persistence;


public class InventorRepository(InventorDbContext dbContext) : IRepository
{
    private readonly InventorDbContext _dbContext = dbContext;


    public async Task AddAsync(Inventor identity)
    {
        _dbContext.Inventors.Add(identity);
        await _dbContext.SaveChangesAsync();
    }
}