using System.Data;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

using Orders.Domain;

using Shared.Annotations;

namespace Orders.Infrastructure.Persistence;


public class OrderRepository(OrderDbContext dbContext) : IRepository
{
    private readonly OrderDbContext _dbContext = dbContext;


    public async Task AddAsync(Order identity)
    {
        _dbContext.Orders.Add(identity);
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.SaveChangesAsync(cancellationToken);
    }
}