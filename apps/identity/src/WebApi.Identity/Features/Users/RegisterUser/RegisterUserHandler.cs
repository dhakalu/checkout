using FluentValidation;
using WebApi.Identity.Features.Users.Data;
using WebApi.Identity.Features.Users.Dto;
using WebApi.Identity.Features.Users.RegisterUser;

namespace WebApi.Identity.Features.Users.RegisterUser;

public class RegisterUserHandler
{

    public static Task<RegisterUserResponse> HandleAsync(RegisterUserRequest request, 
    RegisterUserRequestValidator validator,
    RegisterUserCommand cmd, CancellationToken cancellationToken)
    {
        validator.ValidateAndThrow(request);
        return cmd.Execute(request, cancellationToken);
    }
}