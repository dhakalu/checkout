using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using WebApi.Identity.Features.Users.Data;


namespace WebApi.Identity.Features.Auth.Token;

public class TokenProvider(IConfiguration configuration)
{

    private readonly IConfiguration _configuration = configuration;
    public string Create(User user)
    {

        var signingSecret = _configuration["Jwt:Secret"];
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingSecret!));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor()
        {
            Subject = new ClaimsIdentity([
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email)
          ]),
            Expires = DateTime.UtcNow.Add(TimeSpan.FromMinutes(15)),
            SigningCredentials = credentials,
            Issuer = "identity.checkout.com",
            Audience = "checkout.com"
        };


        var handler = new JsonWebTokenHandler();

        return handler.CreateToken(tokenDescriptor);


    }
}