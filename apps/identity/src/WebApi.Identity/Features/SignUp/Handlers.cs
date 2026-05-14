using FluentValidation;
using WebApi.Identity.Features.Signup.Dto;
using WebApi.Identity.Features.Signup.Validators;

namespace WebApi.Identity.Features.Signup;

public class SignupHandlers
{

    public static Task<SignupResponse> HandleSignUpAsync(SignUpRequest request, 
    SignupRequestValidator validator,
    ISignupService signupService, CancellationToken cancellationToken)
    {
        validator.ValidateAndThrow(request);
        return signupService.SignUp(request, cancellationToken);
    }
}