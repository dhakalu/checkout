using Microsoft.AspNetCore.Identity;
using WebApi.Identity.Features.Auth.Dto;
using WebApi.Identity.Features.Users.Data;

namespace WebApi.Identity.Features.Auth.Token;

public class IssueTokenCommand(ILogger<IssueTokenCommand> logger, IdentityRepository identityRepository, IPasswordHasher<string> passwordHasher)
{

    private readonly IPasswordHasher<string> _passwordHasher = passwordHasher;
    private readonly ILogger<IssueTokenCommand> _logger = logger;
    private readonly IdentityRepository _identityRepository = identityRepository;

    public async Task<AuthorizeResponse> Execute(AuthorizeRequest request, CancellationToken cancellationToken)
    {
        var identity = await _identityRepository.GetIdentityByEmailAsync(request.Email, cancellationToken);

        if (identity == null)
        {
            _logger.LogInformation("Attempted logging in with wrong email {email}", request.Email);
            throw new UnauthorizedAccessException("Invalid email or password");
        }
        
        var result = _passwordHasher.VerifyHashedPassword("user-placeholder", identity.PasswordHash, request.Password);
        if (result != PasswordVerificationResult.Success)
        {
          throw new UnauthorizedAccessException("Invalid email or password");  
        }
        await GenerateAccessToken(identity, cancellationToken);
        return new AuthorizeResponse(identity.FirstName, identity.Email);   
    }

    public async Task<string> GenerateAccessToken(WebApi.Identity.Features.Users.Data.Identity identity, CancellationToken cancellationToken)
    {
        
        return "";
    }
    
}