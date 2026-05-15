namespace WebApi.Identity.Features.Users.Data;

using Microsoft.EntityFrameworkCore;

public class UserRepository(IdentityDbContext dbContext)
{
    private readonly IdentityDbContext _dbContext = dbContext;


    public async Task AddIdentityAsync(User identity)
    {
        _dbContext.Identities.Add(identity);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<User?> GetIdentityByIdAsync(string id, CancellationToken cancellationToken)
    {
        return await _dbContext.Identities.FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
    }
    public async Task<User?> GetIdentityByEmailAsync(string email, CancellationToken cancellationToken)
    {
        return await _dbContext.Identities.FirstOrDefaultAsync(i => i.Email == email, cancellationToken);
    }
}