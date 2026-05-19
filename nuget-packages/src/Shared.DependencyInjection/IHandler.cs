
namespace Shared.DependencyInjection;

public interface IHandler
{
}
public interface IHandler2<TRequest, TResponse>
{
    public Task<TResponse> HandleAsync(TRequest rq, TResponse re);
}
