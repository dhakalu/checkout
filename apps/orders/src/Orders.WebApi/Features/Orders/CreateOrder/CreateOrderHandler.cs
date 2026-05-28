
using Orders.Domain;
using Orders.Infrastructure.Persistence;
using Shared.Annotations;

namespace Orders.WebApi.Features.Orders.CreateOrder;

public class CreateOrderHandler(OrderRepository orderRepository) : IHandler
{

    private OrderRepository _orderRepository = orderRepository;

    public async Task HandleAsync(CreateOrderCommand cmd, CancellationToken cancellationToken)
    {
        var items = cmd.Items.Select(i => new OrderItem(
            new Guid(),
            i.ProductId,
            i.ProductName,
            i.Sku,
            i.UnitPrice,
            i.Quantity
        ));
        var shippingAddress = new ShippingAddress(
            cmd.Address.Street,
            cmd.Address.City,
            cmd.Address.State,
            cmd.Address.ZipCode
        );
        var order = new Order(items, OrderStatus.Pending, shippingAddress);
        await _orderRepository.AddAsync(order);
    }
}