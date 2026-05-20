using Microsoft.AspNetCore.Routing;

namespace Shared.Annotations;

public interface IEndpoint
{
    void MapEndpoints(IEndpointRouteBuilder app);
}