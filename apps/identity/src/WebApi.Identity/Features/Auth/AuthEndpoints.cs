namespace WebApi.Identity.Features.Auth;
using WebApi.Identity;
using WebApi.Identity.Features.Auth.Token;

public class AuthEndpoints: IEndpoint
{

    public const string BasePath = "/authorize";
    public void MapEndpoints(IEndpointRouteBuilder route)
    {
        route.MapPost("/authorize", IssueTokenHandler.HandleAsync);
    }
}