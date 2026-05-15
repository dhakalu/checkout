using System.Net;
using WebApi.Identity.Features.Clients;

namespace WebApi.Identity.Tests.Features.Clients;

public class RegisterClientEndpointTests()
{
    

    [Fact]
    public async Task When_RegisterClient_Is_Called_WithValidRequest_Returns_Ok()
    {
        var app = new IdentityWebApplicationFactory();
        var client = app.CreateClient();

        var result = await client.PostAsJsonAsync(RegisterClientEndpoint.Path, new {}, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Created, result.StatusCode);
    }
}