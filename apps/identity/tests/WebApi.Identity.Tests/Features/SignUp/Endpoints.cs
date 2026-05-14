using System.Net;
using Microsoft.AspNetCore.Mvc;

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
    public async Task When_SignUp_Is_Called_With_Empty_Fields_Returns_BadRequest()
    {
        var app = new IdentityWebApplicationFactory();
        var client = app.CreateClient();
        var result = await client.PostAsJsonAsync("/signup", new
        {
            Email = "",
            Password = "",
            firstName = "",
            lastName = ""
        }, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);

        await result.Content.ReadFromJsonAsync<ValidationProblemDetails>(cancellationToken: TestContext.Current.CancellationToken)
            .ContinueWith(validationResult =>
            {
                var validationErrors = validationResult.Result?.Errors;
                Assert.NotNull(validationErrors);
                Assert.Contains("email", validationErrors.Keys);
                Assert.Contains("password", validationErrors.Keys);
                Assert.Contains("firstName", validationErrors.Keys);
                Assert.Contains("lastName", validationErrors.Keys);

                Assert.Contains("Email is required.", validationErrors["email"]);
                Assert.Contains("Password is required.", validationErrors["password"]);
                Assert.Contains("First name is required.", validationErrors["firstName"]);
                Assert.Contains("Last name is required.", validationErrors["lastName"]);
            }, TestContext.Current.CancellationToken);
    }

}