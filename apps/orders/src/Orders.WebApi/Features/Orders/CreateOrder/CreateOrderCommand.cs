
using System.Collections.ObjectModel;

using Orders.Contracts;

namespace Orders.WebApi.Features.Orders.CreateOrder;

public record CreateOrderCommand(ShippingAddress Address, ReadOnlyCollection<OrderItem> Items);