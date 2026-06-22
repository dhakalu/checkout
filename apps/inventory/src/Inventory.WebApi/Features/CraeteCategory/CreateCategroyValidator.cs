using FluentValidation;

using Inventory.Contracts;

namespace Inventory.WebApi.Features.CraeteCategory;

public class CreateCategoryValidator : AbstractValidator<CreateCategoryRequest>
{

    public const string NameIsRequiredMessage = "Name is required";
    public const string SlungIsRequiredMessage = "Slug is required";
    public const string DescriptionIsRequiredMessage = "Description is required";
    public CreateCategoryValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage(NameIsRequiredMessage);
        RuleFor(x => x.Description).NotEmpty().WithMessage(DescriptionIsRequiredMessage);
        RuleFor(x => x.Slug).NotEmpty().WithMessage(SlungIsRequiredMessage);
    }
}