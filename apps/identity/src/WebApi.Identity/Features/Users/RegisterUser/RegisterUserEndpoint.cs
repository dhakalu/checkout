using FluentValidation;
using Shared.Annotations;

namespace WebApi.Identity.Features.Users.RegisterUser;


public class RegisterUserEndpoint : IEndpoint
{
    public const string Path = "/users";

    public void MapEndpoints(IEndpointRouteBuilder app)
    {

        var group = app.MapGroup(Path);

        group.MapPost("", HandleRegisterUserAsync)
        .WithName("RegisterUser");
    }

    private static Task<RegisterUserResponse> HandleRegisterUserAsync(RegisterUserRequest request,
    RegisterUserRequestValidator validator,
    RegisterUserHandler handler, CancellationToken cancellationToken)
    {
        validator.ValidateAndThrow(request);
        var cmd = new RegisterUserCommand(request.Password, request.Email, request.FirstName, request.LastName);
        return handler.Handle(cmd, cancellationToken);
    }
}