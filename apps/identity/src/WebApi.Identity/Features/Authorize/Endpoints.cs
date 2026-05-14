namespace WebApi.Identity.Features.Authorize;

public static class AuthorizeEndpoints
{
    public static void MapAuthorizeEndpoints(this WebApplication app)
    {
        app.MapPost("/authorize", AuthorizeHandlers.HandleAuthorizeAsync);
    }
}