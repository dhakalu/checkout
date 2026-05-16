namespace WebApi.Identity.Features.Users.Data;

using Microsoft.EntityFrameworkCore;

public class UserRepository(IdentityDbContext dbContext)
{
    private readonly IdentityDbContext _dbContext = dbContext;


    public async Task AddAsync(User identity)
    {
        _dbContext.Identities.Add(identity);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.Identities.FirstOrDefaultAsync(i => i.Id == id.ToString(), cancellationToken);
    }
    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken)
    {
        return await _dbContext.Identities.FirstOrDefaultAsync(i => i.Email == email, cancellationToken);
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken)
    {
        return await _dbContext.Identities.AnyAsync(u => u.Email == email, cancellationToken);
    }

    internal async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        int noOfRows = await _dbContext.Identities.Where(i => i.Id == id.ToString()).ExecuteDeleteAsync(cancellationToken);
        return noOfRows > 0;
    }
}