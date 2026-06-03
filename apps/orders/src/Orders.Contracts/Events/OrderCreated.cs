using System.Collections.ObjectModel;

namespace Orders.Contracts.Events;

public record OrderCreated(Guid OrderId, Guid CustomerId, ShippingAddress ShippingAddress, IReadOnlyList<OrderItem> Items);