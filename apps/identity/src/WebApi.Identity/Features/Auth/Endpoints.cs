namespace WebApi.Identity.Features.Auth;

public static class AuthorizeEndpoints
{
    public static void MapAuthorizeEndpoints(this WebApplication app)
    {
        app.MapPost("/authorize", AuthorizeHandlers.HandleAuthorizeAsync);
    }
}