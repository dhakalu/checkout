using Microsoft.EntityFrameworkCore;
using WebApi.Identity.Features.Clients.GetClient;

namespace WebApi.Identity.Features.Clients.Data;

public class ClientRepository(IdentityDbContext dbContext)
{

    private readonly IdentityDbContext _dbContext = dbContext;

    public async Task<Guid> SaveAsync(Client client, CancellationToken cancellationToken)
    {
        await _dbContext.Clients.AddAsync(client, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return client.Id;
    }

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken)
    {
        return await _dbContext.Clients.AnyAsync(c => c.Name == name, cancellationToken);
    }

    public async Task<GetClientResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext
            .Clients
            .Where(c => c.Id == id)
            .Select(c => new GetClientResponse
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                IsActive = c.IsActive,
                Scopes = c.Scopes.Select(s => s.ScopeKey).ToList()
            })
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }


    public async Task<bool> UpdateScopesAsync(Guid id, IReadOnlyCollection<string> scopes, CancellationToken cancellationToken)
    {
        var client = await _dbContext
            .Clients
            .Include(c => c.Scopes)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken) ?? throw new KeyNotFoundException("Client with gievn id does not exist.");

        client.Scopes.Clear();

        client.Scopes.AddRange(scopes.Select(s => new ClientScope { ScopeKey = s }));
        var c = await _dbContext.SaveChangesAsync(cancellationToken);
        return c > 0;
    }

    internal async Task<int> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.Clients.Where(c => c.Id == id).ExecuteDeleteAsync(cancellationToken);
    }
}