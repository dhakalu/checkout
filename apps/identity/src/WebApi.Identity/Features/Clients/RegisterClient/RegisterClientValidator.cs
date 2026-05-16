using System;
using FluentValidation;

namespace WebApi.Identity.Features.Clients.RegisterClient;

public class RegisterClientValidator : AbstractValidator<RegisterClientRequest>
{


    public RegisterClientValidator()
    {
        RuleFor(c => c.Name)
            .NotEmpty()
            .WithMessage("Name is required.")
            .MaximumLength(100)
            .WithMessage("Name must be shorter than 100 characters.");

        RuleFor(c => c.Description)
            .NotEmpty()
            .WithMessage("Description is required.")
            .MaximumLength(500)
            .WithMessage("Description must be shorter than 500 characters.");
    }

}
