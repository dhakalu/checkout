using System;
using FluentValidation;
using WebApi.Identity.Features.Auth.Dto;

namespace WebApi.Identity.Features.Auth.Validators;

public class AuthorizeRequestValidator: AbstractValidator<AuthorizeRequest>
{

    public AuthorizeRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long.");

    }

}
