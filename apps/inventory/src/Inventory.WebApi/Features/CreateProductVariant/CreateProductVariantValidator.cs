using FluentValidation;

using Inventory.Contracts;

namespace Inventory.WebApi.Features.CreateProductVariant;

public class CreateProductVariantValidator : AbstractValidator<CreateProductVariantRequest>
{
    public CreateProductVariantValidator()
    {


        RuleFor(x => x.Sku).NotEmpty().MaximumLength(50);

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Product name is required")
            .MaximumLength(100)
            .WithMessage("Product name must be shorter than 100 characters.");

        RuleFor(x => x.Price)
            .GreaterThan(0);

        RuleFor(x => x.Cost)
            .GreaterThan(0);

        RuleFor(x => x.ComparePrice)
            .GreaterThan(0);
    }
}