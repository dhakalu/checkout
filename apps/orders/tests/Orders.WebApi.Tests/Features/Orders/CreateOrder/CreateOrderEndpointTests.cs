using System.Net;
using System.Net.Sockets;
using Microsoft.AspNetCore.Mvc;
using Orders.Contracts;
using Orders.WebApi.Features.Orders.CreateOrder;
using Testing.Shared;
using Xunit;

namespace Orders.WebApi.Tests.Features.Orders.CreateOrder;

public class CreateOrderEndpointTests(OrdersWebApplicationFactory app) : IClassFixture<OrdersWebApplicationFactory>
{
    private readonly HttpClient _client = app.CreateClient();

    private readonly ShippingAddress _validAddress = new ShippingAddress
    {
        Street = "123 Main St",
        City = "Anytown",
        State = "CA",
        ZipCode = "12345"
    };


    [Fact]
    public async Task CreateOrder_ValidRequest_Returns_Created()
    {
        var request = new CreateOrderRequest
        {
            CustomerId = Guid.NewGuid(),
            ShippingAddress = _validAddress,
            Items = [
                new()
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Abc",
                    Sku = "abc",
                    Quantity = 2,
                    UnitPrice = 19.99m
                }
            ]
        };

        // Act
        var response = await _client.PostAsJsonAsync("/orders", request, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task CreateOrder_InvalidRequest_ReturnsBadRequest()
    {
        var req = new CreateOrderRequest { };
        var cancellationToken = TestContext.Current.CancellationToken;
        var response = await _client.PostAsJsonAsync(CreateOrderEndpoint.Path, req, cancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var responseBody = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(cancellationToken);
        Assert.NotNull(responseBody);
        responseBody.IsValid(["shippingAddress", "items"], [CreateOrderValidator.addressIsRequired, CreateOrderValidator.itemsAreRequired]);
    }

    [Fact]
    public async Task CreateOrder_InvalidItems_ReturnsBadRequest()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var req = new CreateOrderRequest
        {
            ShippingAddress = _validAddress,
            Items = [
                new OrderItem
                {
                   Quantity = 0,
                   UnitPrice = -1
                },
            ]
        };
        var response = await _client.PostAsJsonAsync(CreateOrderEndpoint.Path, req, cancellationToken);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var responseBody = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(cancellationToken);
        Assert.NotNull(responseBody);
        string[] expectedErrorKeys = ["items[0].productId", "items[0].quantity"];
        string[] errors = [CreateOrderValidator.productIdIsRequired, CreateOrderValidator.quantityMustBeValid];
        responseBody.IsValid(expectedErrorKeys, errors);
    }

    [Fact]
    public async Task CreateOrder_InvalidShippingAddress_ReturnsBadRequest()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var req = new CreateOrderRequest
        {
            ShippingAddress = new ShippingAddress
            {

            },
            Items = [
                new OrderItem
                {
                   ProductId = Guid.NewGuid(),
                   Quantity = 10,
                   UnitPrice = 1
                },
            ]
        };
        var response = await _client.PostAsJsonAsync(CreateOrderEndpoint.Path, req, cancellationToken);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var responseBody = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(cancellationToken);
        Assert.NotNull(responseBody);
        string[] expectedErrorKeys = ["shippingAddress.street", "shippingAddress.city", "shippingAddress.state", "shippingAddress.zipCode"];
        string[] errors = [CreateOrderValidator.streetAddressIsRequired, CreateOrderValidator.cityIsRequired, CreateOrderValidator.stateMustBeValidUsState, CreateOrderValidator.zipIsRequired];
        responseBody.IsValid(expectedErrorKeys, errors);
    }
}