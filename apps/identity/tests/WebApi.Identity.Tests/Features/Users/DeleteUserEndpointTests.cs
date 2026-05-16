using System;
using System.Net;
using WebApi.Identity.Features.Users.RegisterUser;
using WebApi.Identity.Tests;

namespace WebApi.Identity.Tests.Features.Users;

public class DeleteUserEndpointTests(IdentityWebApplicationFactory app) : IClassFixture<IdentityWebApplicationFactory>
{
    private readonly HttpClient _client = app.CreateClient();

    [Fact]
    public async Task DeleteUser_NonExistingUuid_Returns500()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var result = await _client.DeleteAsync("/users/7f661577-cd8f-48c7-89d4-c115dfc5dd0d", cancellationToken);
        // Intentionally returing 500, more specific error 
        // gives information like UUID does not exist, etc
        Assert.Equal(HttpStatusCode.InternalServerError, result.StatusCode);
    }

    [Fact]
    public async Task DeleteUser_ValidUuid_Returns204()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var createUserResult = await _client.PostAsJsonAsync(RegisterUserEndpoint.Path, new RegisterUserRequest(
        "Password123!", "delete_user_success@example.com", "Test", "Test"), cancellationToken);
        createUserResult.EnsureSuccessStatusCode();
        var createUserRequestResponse = await createUserResult.Content.ReadFromJsonAsync<RegisterUserResponse>(cancellationToken);
        Assert.NotNull(createUserRequestResponse);
        var result = await _client.DeleteAsync($"/users/{createUserRequestResponse.Id}", cancellationToken);
        Assert.Equal(HttpStatusCode.NoContent, result.StatusCode);
    }

    [Fact]
    public async Task DeleteUser_InvalidUuid_Returns400()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var result = await _client.DeleteAsync("/users/invalid", cancellationToken);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    }
}
