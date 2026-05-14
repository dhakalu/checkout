using System;
using FluentValidation;
using WebApi.Identity.Features.Signup.Dto;

namespace WebApi.Identity.Features.Signup.Validators;

public class SignupRequestValidator: AbstractValidator<SignUpRequest>
{

    public SignupRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long.");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.");
        
        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.");
    }

}
