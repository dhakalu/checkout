using Microsoft.AspNetCore.Routing;

namespace Shared.DependencyInjection;

public interface IEndpoint
{
    void MapEndpoints(IEndpointRouteBuilder app);
}