namespace WebApi.Identity.Features.Users.RegisterUser;

using Microsoft.AspNetCore.Identity;
using Shared.Annotations;
using WebApi.Identity.Features.Users.Data;
public class RegisterUserHandler(ILogger<RegisterUserHandler> logger,
    IPasswordHasher<string> passwordHasher,
    UserRepository identityRepository) : IHandler
{
    private readonly ILogger<RegisterUserHandler> _logger = logger;
    private readonly IPasswordHasher<string> _passwordHasher = passwordHasher;

    private readonly UserRepository _identityRepository = identityRepository;
    public async Task<RegisterUserResponse> Handle(RegisterUserCommand cmd,
     CancellationToken cancellationToken)
    {
        // check if the email is already registered
        var exists = await _identityRepository.ExistsByEmailAsync(cmd.Email, cancellationToken);
        if (exists)
        {
            _logger.LogWarning("Attempt to register with an already registered email: {Email}", cmd.Email);
            throw new InvalidOperationException("Email is already registered.");
        }

        if (string.IsNullOrWhiteSpace(cmd.Email) || string.IsNullOrWhiteSpace(cmd.Password))
        {
            throw new ArgumentException("Email and password are required.");
        }

        var id = Guid.NewGuid().ToString();
        await _identityRepository.AddAsync(new User
        {
            Id = id,
            Email = cmd.Email,
            // todo - keep salt in the keyvault and use it here to hash the password
            PasswordHash = _passwordHasher.HashPassword("user-placeholder", cmd.Password),
            FirstName = cmd.FirstName,
            LastName = cmd.LastName
        });
        // Return a dummy response with a new user ID
        return new RegisterUserResponse { Id = id };
    }
}