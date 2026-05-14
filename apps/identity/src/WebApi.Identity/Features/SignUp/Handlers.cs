using WebApi.Identity.Features.Signup.Dto;

namespace WebApi.Identity.Features.Signup;

public class SignupHandlers
{

    public static Task<SignupResponse> HandleSignUpAsync(SignUpRequest request, ISignupService signupService, CancellationToken cancellationToken)
    {
        return signupService.SignUp(request, cancellationToken);
    }
}