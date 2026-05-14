using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Identity.Tests.Features.Authorize;


public class EndpointsTests

{
    private readonly string sighnUpEndpoint = "/signup";
    private readonly string authorizeEndpoint = "/authorize";
    #region Authorize Tests
    [Fact]
    public async Task When_Authorize_Is_Called_With_Valid_Credentials_Returns_Ok()
    {
        var app = new IdentityWebApplicationFactory();
        var client = app.CreateClient();

        var createResult = await client.PostAsJsonAsync(sighnUpEndpoint, new
        {
            Email = "test_authorize_success@example.com",
            Password = "Password123!",
            firstName = "Test",
            lastName = "User"
        }, TestContext.Current.CancellationToken);
        createResult.EnsureSuccessStatusCode();

        var result = await client.PostAsJsonAsync(authorizeEndpoint, new
        {
            Email = "test_authorize_success@example.com",
            Password = "Password123!"
        }, TestContext.Current.CancellationToken);

        result.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task When_Authorize_Is_Called_With_Invalid_Credentials_Returns_Unauthorized()
    {
        var app = new IdentityWebApplicationFactory();
        var client = app.CreateClient();
 
        var createResult = await client.PostAsJsonAsync(sighnUpEndpoint, new
        {
            Email = "test_authorize_invalid_password@example.com",
            Password = "Password123!",
            firstName = "Test",
            lastName = "User"
        }, TestContext.Current.CancellationToken);
        createResult.EnsureSuccessStatusCode();

        var result = await client.PostAsJsonAsync(authorizeEndpoint, new
        {
            Email = "test_authorize_invalid_password@example.com",
            Password = "InvalidPassword!"
        }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
    }

    [Fact]
    public async Task When_Authorize_Is_Called_With_Nonexistent_Email_Returns_Unauthorized()
    {
        var app = new IdentityWebApplicationFactory();
        var client = app.CreateClient();

        var result = await client.PostAsJsonAsync(authorizeEndpoint, new
        {
            Email = "nonexistent@example.com",
            Password = "Password123!"
        }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
    }

    [Fact]
    public async Task When_Authorize_Is_Called_With_Empty_Fields_Returns_BadRequest()
    {
        var app = new IdentityWebApplicationFactory();
        var client = app.CreateClient();

        var result = await client.PostAsJsonAsync(authorizeEndpoint, new
        {
            Email = "",
            Password = ""
        }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);

        var validationErrors = await result.Content.ReadFromJsonAsync<ValidationProblemDetails>(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(validationErrors);
        Assert.Contains("email", validationErrors.Errors.Keys);
        Assert.Contains("password", validationErrors.Errors.Keys);
        Assert.Contains("Email is required.", validationErrors.Errors["email"]);
        Assert.Contains("Password is required.", validationErrors.Errors["password"]);
    }
    #endregion
}