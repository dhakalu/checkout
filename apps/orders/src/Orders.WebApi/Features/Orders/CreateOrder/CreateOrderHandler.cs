
using MassTransit;

using Orders.Contracts.Events;
using Orders.Domain;
using Orders.Infrastructure.Persistence;

using Shared.Annotations;

namespace Orders.WebApi.Features.Orders.CreateOrder;

public class CreateOrderHandler(
    OrderRepository orderRepository,
    IPublishEndpoint publishEndpoint,
    IUnitOfWork unitOfWork
) : IHandler
{

    private readonly IPublishEndpoint _publishEndpoint = publishEndpoint;

    private readonly OrderRepository _orderRepository = orderRepository;

    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task HandleAsync(CreateOrderCommand cmd, CancellationToken cancellationToken)
    {
        var items = cmd.Items.Select(i => new OrderItem(
            Guid.NewGuid(),
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
        await _publishEndpoint.Publish(new OrderCreated(
            order.Id,
            order.CustomerId,
            cmd.Address,
            cmd.Items
        ), cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}