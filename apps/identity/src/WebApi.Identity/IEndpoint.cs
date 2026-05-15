namespace WebApi.Identity;


public interface IEndpoint
{
    void MapEndpoints(IEndpointRouteBuilder app);
}