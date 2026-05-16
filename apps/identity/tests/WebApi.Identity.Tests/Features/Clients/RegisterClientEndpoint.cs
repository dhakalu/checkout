using System.Net;
using Microsoft.AspNetCore.Mvc;
using WebApi.Identity.Features.Clients.RegisterClient;

namespace WebApi.Identity.Tests.Features.Clients;

public class RegisterClientEndpointTests(IdentityWebApplicationFactory app) : IClassFixture<IdentityWebApplicationFactory>
{

    private readonly HttpClient _client = app.CreateClient();


    [Fact]
    public async Task RegisterClient_ValidRequest_Returns201()
    {
        var result = await _client.PostAsJsonAsync(RegisterClientEndpoint.Path, new RegisterClientRequest
        {
            Name = "Test Valid Request",
            Description = "Created to test valid test request"
        }, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Created, result.StatusCode);
    }

    [Fact]
    public async Task RegisterClient_EmptyFieldsRequest_Returns_BadRequest()
    {

        var result = await _client.PostAsJsonAsync(RegisterClientEndpoint.Path, new RegisterClientRequest
        {
            Name = "",
            Description = ""
        }, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        var response = await result.Content.ReadFromJsonAsync<ValidationProblemDetails>(TestContext.Current.CancellationToken);
        Assert.NotNull(response);
        Assert.NotNull(response.Errors);

        Assert.Contains("name", response.Errors.Keys);
        Assert.Contains("description", response.Errors.Keys);

        Assert.Contains("Name is required.", response.Errors["name"]);
        Assert.Contains("Description is required.", response.Errors["description"]);
    }

    [Fact]
    public async Task RegisterClient_LongerFieldsRequest_Returns_BadRequest()
    {

        var result = await _client.PostAsJsonAsync(RegisterClientEndpoint.Path, new RegisterClientRequest
        {
            Name = "H9u2z7Lp4mQ1v8N6x0T9s3Kj5R7w2Y1f8G0v4M6q2N5z8X9l0B4r3S1p7W2t5D0g4K8f9H1j2L3m4N5p6Q7r8S9t0V1w2X3y4Z5a6B7c8D9e0F1g2H3j4L5m6N7p8Q9r0",
            Description = "uY8tN2mQ9vL5xA1cR0fK7zP4eG6hJ3wS1iO9bV4mC2nZ8pX5dY0qL3kF7jH2rT9vA1xS4zM6nB8oP0eW3qG5hK2lJ7fD1sR9tY4uI6oP8vX2bC5nM0zQ4wL7kF1jH9rT3vA5xS8zM2nB6oP0eW4qG7hK1lJ3fD9sR2tY5uI8oP0vX4bC6nM2zQ7wL1kF3jH9rT5vA8xS0zM2nB4oP6eW1qG9hK3lJ7fD2sR5tY8uI0oP4vX6bC2nM9zQ1wL3kF7jH2rT5vA8xS0zM4nB6oP1eW9qG3hK7lJ2fD5sR8tY0uI4oP6vX2bC9nM1zQ3wL7kF2jH5rT8vA0xS4zM6nB1oP9eW3qG7hK2lJ5fD8sR0tY4uI6oP1vX3bC9nM7zQ2wL5kF8jH0rT4vA6xS1zM9nB3oP7eW2qG5hK8lJ0fD4sR6tY1uI9oP3vX7bC2nM5zQ8wL0kF4jH6rT1vA9xS3zM7nB2oP5eW8qG0hK4lJ6fD1sR9tY3uI7oP2vX5bC8nM0zQ4wL1kF6jH9rT2vA5xS8zM0nB4oP7eW1qG9hK3lJ6fD2sR8tY0uI5oP9"
        }, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        var response = await result.Content.ReadFromJsonAsync<ValidationProblemDetails>(TestContext.Current.CancellationToken);
        Assert.NotNull(response);
        Assert.NotNull(response.Errors);

        Assert.Contains("name", response.Errors.Keys);
        Assert.Contains("description", response.Errors.Keys);

        Assert.Contains("Name must be shorter than 100 characters.", response.Errors["name"]);
        Assert.Contains("Description must be shorter than 500 characters.", response.Errors["description"]);
    }

    [Fact]
    public async Task RegisterClient_DuplicateName_Returns_Conflict()
    {

        var result = await _client.PostAsJsonAsync(RegisterClientEndpoint.Path, new RegisterClientRequest
        {
            Name = "Test Duplicate",
            Description = "Client created to test duplicate error"
        }, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Created, result.StatusCode);
        var resultAgain = await _client.PostAsJsonAsync(RegisterClientEndpoint.Path, new RegisterClientRequest
        {
            Name = "Test Duplicate",
            Description = "Client created to test duplicate error attempt 2"
        }, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Conflict, resultAgain.StatusCode);
    }
}