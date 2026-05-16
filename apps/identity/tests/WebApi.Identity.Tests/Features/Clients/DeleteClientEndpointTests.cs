
using System.Net;
using WebApi.Identity.Features.Clients.RegisterClient;

namespace WebApi.Identity.Tests.Features.Clients;

public class DeleteClientEndpointTests(IdentityWebApplicationFactory app) : IClassFixture<IdentityWebApplicationFactory>
{


    private readonly HttpClient _client = app.CreateClient();

    [Fact]
    public async Task DeleteClient_WhenValidGuild_ShouldReturnNoContent()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        // create new client 

        var createClientMessage = await _client.PostAsJsonAsync(RegisterClientEndpoint.Path, new RegisterClientRequest
        {
            Name = "DeleteClient_WhenValidGuild_ShouldReturnNoContent",
            Description = "DeleteClient_WhenValidGuild_ShouldReturnNoContent",
            IsActive = false
        }, cancellationToken);
        Assert.Equal(HttpStatusCode.Created, createClientMessage.StatusCode);
        var createdClient = await createClientMessage.Content.ReadFromJsonAsync<RegisterClientResponse>(cancellationToken);
        Assert.NotNull(createdClient);
        // delete new client 

        var deleteClientMessage = await _client.DeleteAsync($"/clients/{createdClient.Id}", cancellationToken);
        Assert.Equal(HttpStatusCode.NoContent, deleteClientMessage.StatusCode);
        // fetch client detail to make sure its 404
        var fetchClientMessage = await _client.GetAsync($"/clients/{createdClient.Id}", cancellationToken);
        Assert.Equal(HttpStatusCode.NotFound, fetchClientMessage.StatusCode);


    }

    [Fact]
    public async Task DeleteClient_NonExistingClient_ShouldReturnNotFound()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var id = Guid.NewGuid();
        var deleteClientMessage = await _client.DeleteAsync($"/clients/{id}", cancellationToken);
        Assert.Equal(HttpStatusCode.NotFound, deleteClientMessage.StatusCode);
    }

    [Fact]
    public async Task DeleteClient_InvalidGuid_ShouldReturnBadRequest()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var id = Guid.NewGuid();
        var deleteClientMessage = await _client.DeleteAsync($"/clients/test-client", cancellationToken);
        Assert.Equal(HttpStatusCode.BadRequest, deleteClientMessage.StatusCode);
    }

}
