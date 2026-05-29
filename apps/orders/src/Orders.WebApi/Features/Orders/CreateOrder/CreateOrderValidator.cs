
using System.Data;

using FluentValidation;

using Orders.Contracts;

namespace Orders.WebApi.Features.Orders.CreateOrder;

public class CreateOrderValidator : AbstractValidator<CreateOrderRequest>
{
    public const string cityIsRequired = "City is required.";
    public const string addressIsRequired = "Address is required.";
    public const string itemsAreRequired = "One or more items are required.";

    public const string productIdIsRequired = "Product identifier is required.";

    public const string productNameIsRequired = "Product name is required.";

    public const string quantityMustBeValid = "Quantity must be 1 or greater.";

    public const string unitPriceMustBeValid = "Unit price must greater than 0.";

    public const string stateMustBeValidUsState = "State must be a valid US state.";

    public const string streetAddressIsRequired = "Street address is required.";

    public const string zipMustBeValidFormat = "Zip code must be a valid US format (e.g. 12345 or 12345-6789).";

    public const string zipIsRequired = "Zip code is required.";
    public const string productSkuIsRequired = "Product sku is required.";

    public CreateOrderValidator()
    {
        RuleFor(x => x.Items).NotEmpty().WithMessage(itemsAreRequired).DependentRules(() =>
        {
            RuleForEach(x => x.Items).ChildRules(item =>
            {
                item.RuleFor(i => i.ProductId).NotEmpty().WithMessage(productIdIsRequired);
                item.RuleFor(i => i.ProductName).NotEmpty().WithMessage(productNameIsRequired);
                item.RuleFor(i => i.Sku).NotEmpty().WithMessage(productSkuIsRequired);
                item.RuleFor(i => i.Quantity).GreaterThan(0).WithMessage(quantityMustBeValid);
                item.RuleFor(i => i.UnitPrice).GreaterThan(0).WithMessage(unitPriceMustBeValid);
            });
        });

        RuleFor(x => x.ShippingAddress)
            .NotNull()
            .WithMessage(addressIsRequired)
            .DependentRules(() =>
            {
                RuleFor(x => x.ShippingAddress.Street)
                    .NotEmpty()
                    .WithMessage(streetAddressIsRequired);

                RuleFor(x => x.ShippingAddress.City)
                    .NotEmpty()
                    .WithMessage(cityIsRequired);

                RuleFor(x => x.ShippingAddress.State)
                    .NotEmpty()
                    .WithMessage(stateMustBeValidUsState)
                    .Matches(@"^[A-Z]{2}$")
                    .WithMessage(stateMustBeValidUsState)
                    .When(x => !string.IsNullOrEmpty(x.ShippingAddress.State), ApplyConditionTo.CurrentValidator);

                RuleFor(x => x.ShippingAddress.ZipCode)
                    .NotEmpty()
                    .WithMessage(zipIsRequired)
                    .Matches(@"^\d{5}(-\d{4})?$")
                    .WithMessage(zipMustBeValidFormat)
                    .When(x => !string.IsNullOrEmpty(x.ShippingAddress.ZipCode), ApplyConditionTo.CurrentValidator);
            });
    }
}