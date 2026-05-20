namespace Orders.Domain;

public class Order
{
    public Guid Id {get; init; }

    public string Name {get; init; } = default!;
}