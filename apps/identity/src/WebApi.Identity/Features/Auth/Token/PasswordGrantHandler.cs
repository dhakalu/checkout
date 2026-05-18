using Microsoft.AspNetCore.Identity;
using WebApi.Identity.Features.Auth.Dto;
using WebApi.Identity.Features.Users.Data;

namespace WebApi.Identity.Features.Auth.Token;

public class PasswordGrantHandler(ILogger<PasswordGrantHandler> logger,
    UserRepository identityRepository,
    IPasswordHasher<string> passwordHasher,
    TokenProvider tokenProvider) : IHandler
{

    private readonly IPasswordHasher<string> _passwordHasher = passwordHasher;
    private readonly ILogger<PasswordGrantHandler> _logger = logger;
    private readonly UserRepository _identityRepository = identityRepository;

    private readonly TokenProvider _tokenProvider = tokenProvider;

    public async Task<AuthorizeResponse> Execute(PasswordGrantCommand command, CancellationToken cancellationToken)
    {
        var user = await _identityRepository.GetByEmailAsync(command.Email, cancellationToken);

        if (user == null)
        {
            _logger.LogInformation("Attempted logging in with wrong email {email}", command.Email);
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        // if (!user.IsEmailVerified)
        // {
        //     throw new UnauthorizedAccessException("Please verify your email first.");
        // }

        if (user.IsLocked)
        {
            throw new UnauthorizedAccessException("Account is locked, check with administrator.");
        }

        var result = _passwordHasher.VerifyHashedPassword("user-placeholder", user.PasswordHash, command.Password);
        if (result != PasswordVerificationResult.Success)
        {
            throw new UnauthorizedAccessException("Invalid email or password");
        }
        var token = _tokenProvider.Create(user);
        return new AuthorizeResponse(null, token);
    }

}