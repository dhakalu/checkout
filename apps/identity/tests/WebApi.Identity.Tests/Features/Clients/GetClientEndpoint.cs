using System.Net;
using WebApi.Identity.Features.Clients.RegisterClient;
using WebApi.Identity.Features.Clients.GetClient;

namespace WebApi.Identity.Tests.Features.Clients;

public class GetClientEndpointTests(IdentityWebApplicationFactory app): IClassFixture<IdentityWebApplicationFactory>
{
    private readonly HttpClient _client = app.CreateClient(); 
    

    [Fact]
    public async Task GetClient_ValidId_ReturnsOk()
    {

        var createClient = new RegisterClientRequest
        {
          Name = "Test Client",
          Description = "Created to test the get by id successful",
          IsActive = false  
        };
        var cancellationToken = TestContext.Current.CancellationToken;
        var createClientResult = await _client.PostAsJsonAsync(RegisterClientEndpoint.Path, createClient, cancellationToken);
        Assert.Equal(HttpStatusCode.Created, createClientResult.StatusCode);
        var createClientResponse = await createClientResult.Content.ReadFromJsonAsync<RegisterClientResponse>(cancellationToken);
        Assert.NotNull(createClientResponse);
        var getClientResult = await _client.GetAsync($"/clients/{createClientResponse.Id}", TestContext.Current.CancellationToken);
        getClientResult.EnsureSuccessStatusCode();

        var getClientResponse = await getClientResult.Content.ReadFromJsonAsync<GetClientResponse>(cancellationToken);
        Assert.NotNull(getClientResponse);
        Assert.Equal(createClient.Description, getClientResponse.Description);
        Assert.Equal(createClient.Name, getClientResponse.Name);
        Assert.False(getClientResponse.IsActive);
        Assert.Empty(getClientResponse.Scopes);
    }  

    [Fact]
    public async Task GetClient_InvalidGuidAsId_ReturnsBadRequest()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var getClientResult = await _client.GetAsync("/clients/testid", cancellationToken);
        Assert.Equal(HttpStatusCode.BadRequest, getClientResult.StatusCode);
    }  

}
