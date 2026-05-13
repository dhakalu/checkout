namespace WebApi.Identity.Features.Signup;

using WebApi.Identity.Features.Signup.Dto;
using WebApi.Identity.Features.Signup;

public class SignupService: ISignupService
{
    public async Task<SignupResponse> SignUp(SignUpRequest request, CancellationToken cancellationToken)
    {
        // Simulate some async work, e.g., saving to a database
        await Task.Delay(100, cancellationToken);

        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ArgumentException("Email and password are required.");
        }
        
        // Return a dummy response with a new user ID
        return new SignupResponse { Id = Guid.NewGuid().ToString() };
    }
}