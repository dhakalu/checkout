using WebApi.Identity.Features.Signup.Data;
using WebApi.Identity.Features.Signup.Dto;

namespace WebApi.Identity.Features.Signup;

public interface ISignupService
{
    // Signup registers a new user with the provided details 
    // and returns a SignupResponse containing the new user's ID.
    public Task<SignupResponse> SignUp(SignUpRequest request,
     IdentityRepository identityRepository,
     CancellationToken cancellationToken);
}