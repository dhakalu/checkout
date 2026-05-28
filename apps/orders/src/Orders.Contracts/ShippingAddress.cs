namespace Orders.Contracts;

public record ShippingAddress
{
    public string Street { get; init; } = default!;

    public string City { get; init; } = default!;

    public string State { get; init; } = default!;

    public string ZipCode { get; init; } = default!;
}