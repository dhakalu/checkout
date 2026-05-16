using System;
using FluentValidation;

namespace WebApi.Identity.Features.Scopes.CreateScope;

public class CreateScopeValidator : AbstractValidator<CreateScopeRequest>
{


    public CreateScopeValidator()
    {
        RuleFor(s => s.Key)
            .NotEmpty()
            .WithMessage("Key is required.")
            .MaximumLength(50)
            .WithMessage("Key must be shorter than 50 characters.");

        RuleFor(s => s.Name)
            .NotEmpty()
            .WithMessage("Name is required.")
            .MaximumLength(100)
            .WithMessage("Name must be shorter than 100 characters.");

        RuleFor(s => s.Description)
            .NotEmpty()
            .WithMessage("Description is required.")
            .MaximumLength(500)
            .WithMessage("Description must be shorter than 500 characters.");
    }
}
