namespace WebApi.Identity.Tests.Features.Signup;


public class EndpointsTests
{
    [Fact]
    public async Task When_SignUp_Is_Called_Returns_Ok()
    {
        var app = new IdentityWebApplicationFactory();
        var client = app.CreateClient();

        var result = await client.PostAsJsonAsync("/signup", new
        {
            Email = "test@example.com",
            Password = "Password123!",
            firstName = "Test",
            lastName = "User"
        }, TestContext.Current.CancellationToken);
        result.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task When_SignUp_Is_Called_With_Missing_Fields_Returns_BadRequest()
    {
        var app = new IdentityWebApplicationFactory();
        var client = app.CreateClient();
        var result = await client.PostAsJsonAsync("/signup", new
        {
            Email = "",
            Password = "",
            firstName = "Test",
            lastName = "User"
        }, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    }

}