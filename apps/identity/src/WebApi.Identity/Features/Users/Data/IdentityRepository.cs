namespace WebApi.Identity.Features.Users.Data;

using Microsoft.EntityFrameworkCore;

public class IdentityRepository(IdentityDbContext dbContext, ILogger<IdentityRepository> logger)
{
    private readonly IdentityDbContext _dbContext = dbContext;

    private readonly ILogger<IdentityRepository> _logger = logger;

    public async Task AddIdentityAsync(Identity identity)
    {
        _dbContext.Identities.Add(identity);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<Identity?> GetIdentityByEmailAsync(string email, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Checking for existing identity with email: {Email}", email);
        return await _dbContext.Identities.FirstOrDefaultAsync(i => i.Email == email, cancellationToken);
    }
}