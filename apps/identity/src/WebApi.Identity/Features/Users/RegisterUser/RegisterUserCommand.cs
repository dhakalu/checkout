namespace WebApi.Identity.Features.Users.RegisterUser;

using Microsoft.AspNetCore.Identity;
using WebApi.Identity.Features.Users.Data;
using WebApi.Identity.Features.Users.Dto;
public class RegisterUserCommand(ILogger<RegisterUserCommand> logger,
    IPasswordHasher<string> passwordHasher,
    IdentityRepository identityRepository)
{
    private readonly ILogger<RegisterUserCommand> _logger = logger;
    private readonly IPasswordHasher<string> _passwordHasher = passwordHasher;

    private readonly IdentityRepository _identityRepository = identityRepository;
    public async Task<RegisterUserResponse> Execute(RegisterUserRequest request,
     CancellationToken cancellationToken)
    {
        // check if the email is already registered
        var existingIdentity = await _identityRepository.GetIdentityByEmailAsync(request.Email, cancellationToken);
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
        await _identityRepository.AddIdentityAsync(new Identity
        {
            Id = id,
            Email = request.Email,
            // todo - keep salt in the keyvault and use it here to hash the password
            PasswordHash = _passwordHasher.HashPassword("user-placeholder", request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName
        });
        // Return a dummy response with a new user ID
        return new RegisterUserResponse { Id = id };
    }
}