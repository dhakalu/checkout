namespace Shared.Annotations;

public interface IUnitOfWork
{

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}