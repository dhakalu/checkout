using FluentValidation;
using WebApi.Identity.Features.Users.Data;
using WebApi.Identity.Features.Users.Dto;
using WebApi.Identity.Features.Users.RegisterUser;

namespace WebApi.Identity.Features.Users.RegisterUser;

public class RegisterUserHandler
{

    public static Task<RegisterUserResponse> HandleAsync(RegisterUserRequest request, 
    RegisterUserRequestValidator validator,
    IRegisterUserService signupService, CancellationToken cancellationToken)
    {
        validator.ValidateAndThrow(request);
        return signupService.Register(request, cancellationToken);
    }
}