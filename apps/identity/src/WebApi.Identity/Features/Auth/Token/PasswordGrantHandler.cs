using Microsoft.AspNetCore.Identity;
using WebApi.Identity.Features.Auth.Dto;
using WebApi.Identity.Features.Users.Data;

namespace WebApi.Identity.Features.Auth.Token;

public class PasswordGrantHandler(ILogger<PasswordGrantHandler> logger, UserRepository identityRepository, IPasswordHasher<string> passwordHasher)
{

    private readonly IPasswordHasher<string> _passwordHasher = passwordHasher;
    private readonly ILogger<PasswordGrantHandler> _logger = logger;
    private readonly UserRepository _identityRepository = identityRepository;

    public async Task<AuthorizeResponse> Execute(PasswordGrantCommand command, CancellationToken cancellationToken)
    {
        var identity = await _identityRepository.GetIdentityByEmailAsync(command.Email, cancellationToken);

        if (identity == null)
        {
            _logger.LogInformation("Attempted logging in with wrong email {email}", command.Email);
            throw new UnauthorizedAccessException("Invalid email or password");
        }
        
        var result = _passwordHasher.VerifyHashedPassword("user-placeholder", identity.PasswordHash, command.Password);
        if (result != PasswordVerificationResult.Success)
        {
          throw new UnauthorizedAccessException("Invalid email or password");  
        }
        await GenerateAccessToken(identity, cancellationToken);
        return new AuthorizeResponse(identity.FirstName, identity.Email);   
    }

    public async Task<string> GenerateAccessToken(WebApi.Identity.Features.Users.Data.User identity, CancellationToken cancellationToken)
    {
        
        return "";
    }
    
}