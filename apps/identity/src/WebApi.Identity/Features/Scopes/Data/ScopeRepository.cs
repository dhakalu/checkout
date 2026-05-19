using Microsoft.EntityFrameworkCore;
using Shared.DependencyInjection;

namespace WebApi.Identity.Features.Scopes.Data;

public class ScopeRepository(IdentityDbContext context) : IRepository
{
    private readonly IdentityDbContext _conext = context;
    public async Task<bool> AddAsync(Scope scope, CancellationToken cancellationToken)
    {
        await _conext.Scopes.AddAsync(scope, cancellationToken);
        var nOfChenges = await _conext.SaveChangesAsync(cancellationToken);
        return nOfChenges == 1;
    }

    internal async Task<List<Scope>> ExistsAsync(IReadOnlyCollection<string> scopes, CancellationToken cancellationToken)
    {
        return await _conext.Scopes.Where(p => scopes.Contains(p.Key)).ToListAsync(cancellationToken);
    }

    internal async Task<Scope?> GetByKey(string key, CancellationToken cancellationToken)
    {
        return await _conext.Scopes.FirstOrDefaultAsync(s => s.Key == key, cancellationToken);
    }
}
