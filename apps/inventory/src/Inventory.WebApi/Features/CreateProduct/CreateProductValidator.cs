using FluentValidation;

using Inventory.Contracts;


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
            .WithMessage("Product name is required");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Description is required");
    }
}