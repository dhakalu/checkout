using WebApi.Identity.Features.Clients;
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
        var clientId = "";
        var getClientResult = await _client.GetAsync($"/clients/{clientId}", TestContext.Current.CancellationToken);
        getClientResult.EnsureSuccessStatusCode();

        var getClientResponse = await getClientResult.Content.ReadFromJsonAsync<GetClientResponse>(TestContext.Current.CancellationToken);
        Assert.NotNull(getClientResponse);
        Assert.Equal(createClient.Description, getClientResponse.Description);
        Assert.Equal(createClient.Name, getClientResponse.Name);
        Assert.False(getClientResponse.IsActive);
    }   

}
