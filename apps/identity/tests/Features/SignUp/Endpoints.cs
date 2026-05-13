
namespace Tests.Features.SignUp;
class Endpoints
{
    [Fact]
    public async Task When_Endpoint_Is_Called_Returns_Ok()
    {
        var app = new IdentityWebApplicationFactory();
        var client = app.CreateClient();

        var result = await client.PostAsync("/signup", TestContext.Current.CancellationToken);
        result.EnsureSuccessStatusCode();
    }
}