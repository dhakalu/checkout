using System.Net;
using Microsoft.AspNetCore.Mvc;
using WebApi.Identity.Features.Clients.GetClient;
using WebApi.Identity.Features.Clients.ManageClientScope;
using WebApi.Identity.Features.Clients.RegisterClient;
using WebApi.Identity.Features.Scopes.CreateScope;

namespace WebApi.Identity.Tests.Features.Clients;

public class ManageClientScopeEndpointTests(IdentityWebApplicationFactory app) : IClassFixture<IdentityWebApplicationFactory>
{

    private readonly HttpClient _client = app.CreateClient();

    [Fact]
    public async Task ManageScope_WithInvalidScopes_ReturnsBadRequest()
    {

        var cancellationToken = TestContext.Current.CancellationToken;
        var createClientResponseMessage = await _client.PostAsJsonAsync(RegisterClientEndpoint.Path, new RegisterClientRequest
        {
            Name = "ManageScope_WithInvalidScopes_ReturnsBadRequest",
            Description = "Client created for test - ManageScope_WithInvalidScopes_ReturnsBadRequest",
            IsActive = false
        }, cancellationToken);

        Assert.Equal(HttpStatusCode.Created, createClientResponseMessage.StatusCode);
        var client = await createClientResponseMessage.Content.ReadFromJsonAsync<RegisterClientResponse>(cancellationToken);
        Assert.NotNull(client);
        var result = await _client.PutAsJsonAsync($"/clients/{client.Id}/scopes", new ManageClientScopeRequest(["noscope:exists", "noscope:exists2"]), cancellationToken);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        var validationProblem = await result.Content.ReadFromJsonAsync<ValidationProblemDetails>(cancellationToken);
        Assert.NotNull(validationProblem);
        Assert.Equal("One or more validation errors occurred.", validationProblem.Detail);
        var validationErrors = validationProblem.Errors;
        Assert.Contains("scopes", validationErrors.Keys);
        Assert.Contains("These scopes are invalid: noscope:exists, noscope:exists2.", validationErrors["scopes"]);
    }

    [Fact]
    public async Task ManageScope_WithValidScopes_ReturnsNoContent()
    {

        var cancellationToken = TestContext.Current.CancellationToken;
        var createClientResponseMessage = await _client.PostAsJsonAsync(RegisterClientEndpoint.Path, new RegisterClientRequest
        {
            Name = "ManageScope_WithValidScopes_ReturnsOk",
            Description = "Client created for test - ManageScope_WithValidScopes_ReturnsOk",
            IsActive = false
        }, cancellationToken);

        var create1 = await _client.PostAsJsonAsync(CreateScopeEndpoint.Path, new CreateScopeRequest
        {
            Key = "ManageScope_WithValidScopes_ReturnsNoContent",
            Name = "Test",
            Description = "Test"
        }, cancellationToken);
        Assert.Equal(HttpStatusCode.Created, create1.StatusCode);

        Assert.Equal(HttpStatusCode.Created, createClientResponseMessage.StatusCode);
        var client = await createClientResponseMessage.Content.ReadFromJsonAsync<RegisterClientResponse>(cancellationToken);
        Assert.NotNull(client);
        var result = await _client.PutAsJsonAsync($"/clients/{client.Id}/scopes", new ManageClientScopeRequest(["ManageScope_WithValidScopes_ReturnsNoContent"]), cancellationToken);
        Assert.Equal(HttpStatusCode.NoContent, result.StatusCode);
    }

    [Fact]
    public async Task ManageScope_WithValidScopes_SavesToClient()
    {

        var cancellationToken = TestContext.Current.CancellationToken;

        //create necessary scopes
        var create1 = await _client.PostAsJsonAsync(CreateScopeEndpoint.Path, new CreateScopeRequest
        {
            Key = "ManageScope_WithValidScopes_SavesToClient",
            Name = "Test",
            Description = "Test"
        }, cancellationToken);
        Assert.Equal(HttpStatusCode.Created, create1.StatusCode);

        // create the client
        var createClientResponseMessage = await _client.PostAsJsonAsync(RegisterClientEndpoint.Path, new RegisterClientRequest
        {
            Name = "ManageScope_WithValidScopes_SavesToClient",
            Description = "Client created for test - ManageScope_WithValidScopes_SavesToClient",
            IsActive = false
        }, cancellationToken);

        Assert.Equal(HttpStatusCode.Created, createClientResponseMessage.StatusCode);
        var client = await createClientResponseMessage.Content.ReadFromJsonAsync<RegisterClientResponse>(cancellationToken);
        Assert.NotNull(client);

        // add scopes to the client 
        var result = await _client.PutAsJsonAsync($"/clients/{client.Id}/scopes", new ManageClientScopeRequest(["ManageScope_WithValidScopes_SavesToClient"]), cancellationToken);
        Assert.Equal(HttpStatusCode.NoContent, result.StatusCode);

        // read the details of the client
        var savedClientMessage = await _client.GetAsync($"/clients/{client.Id}", cancellationToken);
        savedClientMessage.EnsureSuccessStatusCode();
        var savedClient = await savedClientMessage.Content.ReadFromJsonAsync<GetClientResponse>(cancellationToken);
        Assert.NotNull(savedClient);
        Assert.NotEmpty(savedClient.Scopes);
        Assert.Single(savedClient.Scopes);
        Assert.Contains("ManageScope_WithValidScopes_SavesToClient", savedClient.Scopes);

    }

     [Fact]
    public async Task ManageScope_WithNewValidScopes_OverridesExisting()
    {

        var cancellationToken = TestContext.Current.CancellationToken;
        // create necessary scopes
        var create1 = await _client.PostAsJsonAsync(CreateScopeEndpoint.Path, new CreateScopeRequest
        {
            Key = "ManageScope_WithNewValidScopes_OverridesExisting",
            Name = "Test",
            Description = "Test"
        }, cancellationToken);
        Assert.Equal(HttpStatusCode.Created, create1.StatusCode);

        var create2 = await _client.PostAsJsonAsync(CreateScopeEndpoint.Path, new CreateScopeRequest
        {
            Key = "updated",
            Name = "Test",
            Description = "Test"
        }, cancellationToken);
        Assert.Equal(HttpStatusCode.Created, create2.StatusCode);


        // create the client
        var createClientResponseMessage = await _client.PostAsJsonAsync(RegisterClientEndpoint.Path, new RegisterClientRequest
        {
            Name = "ManageScope_WithNewValidScopes_OverridesExisting",
            Description = "Client created for test - ManageScope_WithNewValidScopes_OverridesExisting",
            IsActive = false
        }, cancellationToken);
        Assert.Equal(HttpStatusCode.Created, createClientResponseMessage.StatusCode);
        var client = await createClientResponseMessage.Content.ReadFromJsonAsync<RegisterClientResponse>(cancellationToken);
        Assert.NotNull(client);
        
        // add scope to the client
        var result = await _client.PutAsJsonAsync($"/clients/{client.Id}/scopes", new ManageClientScopeRequest(["ManageScope_WithNewValidScopes_OverridesExisting"]), cancellationToken);
        Assert.Equal(HttpStatusCode.NoContent, result.StatusCode);
        // update the scope  
        var updatedMessage = await _client.PutAsJsonAsync($"/clients/{client.Id}/scopes", new ManageClientScopeRequest(["updated"]), cancellationToken);
        Assert.Equal(HttpStatusCode.NoContent, updatedMessage.StatusCode);

        // fetch client details
        var savedClientMessage = await _client.GetAsync($"/clients/{client.Id}", cancellationToken);
        savedClientMessage.EnsureSuccessStatusCode();
        var savedClient = await savedClientMessage.Content.ReadFromJsonAsync<GetClientResponse>(cancellationToken);

        // assert new scopes over riding the old scopes
        Assert.NotNull(savedClient);
        Assert.NotEmpty(savedClient.Scopes);
        Assert.Single(savedClient.Scopes);
        Assert.Contains("updated", savedClient.Scopes);

    }
}