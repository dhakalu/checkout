namespace WebApi.Identity.Features.Signup.Data;

using Microsoft.EntityFrameworkCore;

public class IdentityRepository(IdentityDbContext dbContext)
{
    private readonly IdentityDbContext _dbContext = dbContext;

    public async Task AddIdentityAsync(Identity identity)
    {
        _dbContext.Identities.Add(identity);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<Identity?> GetIdentityByEmailAsync(string email)
    {
        return await _dbContext.Identities.FirstOrDefaultAsync(i => i.Email == email);
    }
}