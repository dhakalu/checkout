using Shared.Annotations;

namespace Orders.Domain;


public class ShippingAddress(string street, string city, string state, string zipCode) : IRepository
{
    public Guid Id { get; private set; }

    public string Street { get; private set; } = street;

    public string City { get; private set; } = city;

    public string State { get; private set; } = state;

    public string ZipCode { get; private set; } = zipCode;

    public Guid OrderId { get; set; }

}