namespace WebApi.Identity.Features.Auth;

using WebApi.Identity.Features.Auth.Token;

public static class AuthEndpoints
{

    public const string BasePath = "/authorize";
    public static void MapAuthorizeEndpoints(this WebApplication app)
    {
        app.MapPost("/authorize", IssueTokenHandler.HandleAsync);
    }
}