using Microsoft.AspNetCore.Identity;
using WebApi.Identity.Features.Auth.Dto;
using WebApi.Identity.Features.Users.Data;

namespace WebApi.Identity.Features.Auth;

public class AuthorizeService(ILogger<AuthorizeService> logger, IdentityRepository identityRepository, IPasswordHasher<string> passwordHasher)
{

    private readonly IPasswordHasher<string> _passwordHasher = passwordHasher;
    private readonly ILogger<AuthorizeService> _logger = logger;
    private readonly IdentityRepository _identityRepository = identityRepository;

    public async Task<AuthorizeResponse> Authorize(AuthorizeRequest request, CancellationToken cancellationToken)
    {
        var identity = await _identityRepository.GetIdentityByEmailAsync(request.Email, cancellationToken) 
            ?? throw new UnauthorizedAccessException("Invalid email or password");
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