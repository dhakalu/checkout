using System.Reflection.Metadata;
using WebApi.Identity.Features.Users.RegisterUser;

namespace WebApi.Identity.Features.Users;


public class SignupEndpoints
{
    public const string Users = "/users";

    public static void MapEndpoints(WebApplication app)
    {

        var group = app.MapGroup(Users);

        group.MapPost(Users, RegisterUserHandler.HandleAsync)
        .WithName("RegisterUser");
    }
}