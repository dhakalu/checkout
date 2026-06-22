using FluentValidation;

using Inventory.Contracts;

namespace Inventory.WebApi.Features.CreateBrand;

public class CreateBrandValidator : AbstractValidator<CreateBrandRequest>
{
    public const string NameIsRequired = "Name is required.";
    public const string DescriptionIsRequired = "Description is required.";

    public const string WebsitUriIsRequired = "Websist URI is required.";

    public const string SlugIsRequired = "Slug is required.";



    public CreateBrandValidator()
    {

        RuleFor(x => x.Name).NotEmpty().WithMessage(NameIsRequired);
        RuleFor(x => x.Description).NotEmpty().WithMessage(DescriptionIsRequired);
        RuleFor(x => x.WebsiteUrl).NotEmpty().WithMessage(SlugIsRequired);
        RuleFor(x => x.Slug).NotEmpty().WithMessage(SlugIsRequired);
    }
}