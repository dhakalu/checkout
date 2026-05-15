using System;
using FluentValidation;

namespace WebApi.Identity.Features.Auth.Token;

public class IssueTokenRequestValidator: AbstractValidator<IssueTokenRequest>
{

    public IssueTokenRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required when grant type is 'password'.")
            .When(p => p.GrantType == "password")
            .EmailAddress().WithMessage("Invalid email format.")
            .When(p => p.GrantType == "password");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required when grant type is 'password'.")
            .When(p => p.GrantType == "password")
            .MinimumLength(8)
            .WithMessage("Password must be at least 8 characters long.")
            .When(p => p.GrantType == "password");
        
        RuleFor(x => x.ClientId)
            .NotEmpty()
            .WithMessage("Client id is required when grant type is 'client_credentials'.")
            .When(p => p.GrantType == "client_credentials");
        
        RuleFor(x => x.ClientSecret)
            .NotEmpty()
            .WithMessage("Client secret is required when grant type is 'client_credentials'.")
            .When(p => p.GrantType == "client_credentials");
    }

}
