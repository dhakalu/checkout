namespace WebApi.Identity.Features.Signup;

using Microsoft.AspNetCore.Identity;
using WebApi.Identity.Features.Signup.Data;
using WebApi.Identity.Features.Signup.Dto;
public class SignupService(ILogger<SignupService> logger, IPasswordHasher<string> passwordHasher) : ISignupService
{
    private readonly ILogger<SignupService> _logger = logger;
    private readonly IPasswordHasher<string> _passwordHasher = passwordHasher;

    public async Task<SignupResponse> SignUp(SignUpRequest request, 
     IdentityRepository identityRepository,
     CancellationToken cancellationToken)
    {
        // check if the email is already registered
        var existingIdentity = await identityRepository.GetIdentityByEmailAsync(request.Email, cancellationToken);
        if (existingIdentity != null) 
        {
            _logger.LogWarning("Attempt to register with an already registered email: {Email}", request.Email);
            throw new InvalidOperationException("Email is already registered.");
        }

        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ArgumentException("Email and password are required.");
        }

        var id = Guid.NewGuid().ToString();
        await identityRepository.AddIdentityAsync(new Identity
        {
            Id = id,
            Email = request.Email,
            // todo - keep salt in the keyvault and use it here to hash the password
            PasswordHash = _passwordHasher.HashPassword("user-placeholder", request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName
        });
        // Return a dummy response with a new user ID
        return new SignupResponse { Id = id };
    }
}