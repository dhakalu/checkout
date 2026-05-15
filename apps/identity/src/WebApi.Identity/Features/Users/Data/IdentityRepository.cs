namespace WebApi.Identity.Features.Users.Data;

using Microsoft.EntityFrameworkCore;

public class IdentityRepository(IdentityDbContext dbContext)
{
    private readonly IdentityDbContext _dbContext = dbContext;


    public async Task AddIdentityAsync(Identity identity)
    {
        _dbContext.Identities.Add(identity);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<Identity?> GetIdentityByIdAsync(string id, CancellationToken cancellationToken)
    {
        return await _dbContext.Identities.FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
    }
    public async Task<Identity?> GetIdentityByEmailAsync(string email, CancellationToken cancellationToken)
    {
        return await _dbContext.Identities.FirstOrDefaultAsync(i => i.Email == email, cancellationToken);
    }
}