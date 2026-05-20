using Microsoft.EntityFrameworkCore;
using Shared.Annotations;
using Orders.Domain;

namespace Orders.Infrastructure.Persistence;


public class OrderRepository(OrderDbContext dbContext) : IRepository
{
    private readonly OrderDbContext _dbContext = dbContext;


    public async Task AddAsync(Order identity)
    {
        _dbContext.Orders.Add(identity);
        await _dbContext.SaveChangesAsync();
    }
}