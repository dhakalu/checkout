using System.Reflection.Metadata;
using WebApi.Identity.Features.Users.RegisterUser;

namespace WebApi.Identity.Features.Users;


public class UsersEndpoints
{
    public const string BasePath = "/users";

    public static void MapEndpoints(WebApplication app)
    {

        var group = app.MapGroup(BasePath);

        group.MapPost("", RegisterUserHandler.HandleAsync)
        .WithName("RegisterUser");
    }
}