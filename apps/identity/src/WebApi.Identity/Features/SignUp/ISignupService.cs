namespace WebApi.Identity.Features.Signup;

using WebApi.Identity.Features.Signup.Dto;

public interface ISignupService
{
    // Signup registers a new user with the provided details 
    // and returns a SignupResponse containing the new user's ID.
    public Task<SignupResponse> SignUp(SignUpRequest request, CancellationToken cancellationToken);
}