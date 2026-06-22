using FluentValidation;

using Inventory.Contracts;

namespace Inventory.WebApi.Features.CreateProduct;

public class CreateProductValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductValidator()
    {

        RuleFor(x => x.BrandId)
            .NotEmpty()
            .WithMessage("Brand identifier is required");

        RuleFor(x => x.CategoryId)
            .NotEmpty()
            .WithMessage("Category identifier is required");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Product name is required")
            .MaximumLength(100)
            .WithMessage("Product name must be shorter than 100 characters.");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Description is required");

        RuleFor(x => x.Slug)
            .NotEmpty()
            .MaximumLength(200);
    }
}