using Microsoft.EntityFrameworkCore;

namespace WebApi.Identity.Features.Clients.Data;

public class ClientRepository(IdentityDbContext dbContext)
{

    private readonly IdentityDbContext _dbContext = dbContext;

    public async Task<bool> SaveAsync(Client client, CancellationToken cancellationToken)
    {
        await  _dbContext.Clients.AddAsync(client, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<Client?> GetByNameAsync(string name, CancellationToken cancellationToken)
    {
       return await _dbContext
        .Clients
        .FirstOrDefaultAsync(c => c.Name == name, cancellationToken);
    }
}