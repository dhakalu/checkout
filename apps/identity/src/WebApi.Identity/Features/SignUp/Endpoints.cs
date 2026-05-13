namespace WebApi.Identity.Features.Signup;


public class SignupEndpoints
{
    public const string SignUp = "/signup";

    public static void MapEndpoints(WebApplication app)
    {
        app.MapPost(SignUp, SignupHandlers.HandleSignUpAsync)
        .WithName("SignUp");
    }
}