using Inventory.Domain;

using Microsoft.EntityFrameworkCore;

using Shared.Annotations;

namespace Inventory.Infrastructure.Persistence;


public class InventorRepository(InventoryDbContext dbContext) : IRepository
{
    private readonly InventoryDbContext _dbContext = dbContext;
}