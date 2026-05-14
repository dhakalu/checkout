using System;
using FluentValidation;
using WebApi.Identity.Features.Auth.Dto;

namespace WebApi.Identity.Features.Auth.Token;

public class IssueTokenRequestValidator: AbstractValidator<AuthorizeRequest>
{

    public IssueTokenRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long.");

    }

}
