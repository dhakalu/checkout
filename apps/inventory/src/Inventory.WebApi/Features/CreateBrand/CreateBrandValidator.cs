using FluentValidation;

using Inventory.Contracts;

namespace Inventory.WebApi.Features.CreateBrand;

public class CreateBrandValidator : AbstractValidator<CreateBrandRequest>
{
    public const string NameIsRequired = "Name is required.";
    public const string NameMaxLengthMessage = "Name must be shorter than 100 characters.";
    public const string DescriptionIsRequired = "Description is required.";

    public const string DescriptionMaxLengthMessage = "Description must be shorter than 1000 characters.";
    public const string WebsitUriIsRequired = "Website Url is required.";

    public const string WebsiteMaxLengthMessage = "Website Url must be shorter than 200 character";
    public const string SlugIsRequired = "Slug is required.";

    public const string SlugMaxLengthMessage = "Slug must be shorter than 255 characters.";




    public CreateBrandValidator()
    {

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(NameIsRequired)
            .MaximumLength(100)
            .WithMessage(NameMaxLengthMessage);
        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage(DescriptionIsRequired)
            .MaximumLength(1000)
            .WithMessage(DescriptionIsRequired);
        RuleFor(x => x.WebsiteUrl)
            .NotEmpty()
            .WithMessage(WebsitUriIsRequired)
            .MaximumLength(200)
            .WithMessage(WebsiteMaxLengthMessage)
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out var outUri)
                 && (outUri.Scheme == Uri.UriSchemeHttp || outUri.Scheme == Uri.UriSchemeHttps))
            .WithMessage("Please enter a valid HTTP/HTTPS URL.");

        RuleFor(x => x.Slug)
            .NotEmpty()
            .WithMessage(SlugIsRequired)
            .MaximumLength(255)
            .WithMessage(SlugMaxLengthMessage);
    }
}