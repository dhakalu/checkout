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

    [Fact]
    public async Task When_SignUp_Is_Called_With_Existing_Email_Returns_Conflict()
    {
        var app = new IdentityWebApplicationFactory();
        var client = app.CreateClient();

        await client.PostAsJsonAsync("/signup", new
        {
            Email = "test@example.com",
            Password = "Password123!",
            firstName = "Test",
            lastName = "User"
        }, TestContext.Current.CancellationToken);

        // Then, try to sign up with the same email
        var result2 = await client.PostAsJsonAsync("/signup", new
        {
            Email = "test@example.com",
            Password = "AnotherPassword123!",
            firstName = "Another",
            lastName = "User"
        }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, result2.StatusCode);
    }

    #region  Login
    [Fact]
    public async Task When_Login_Is_Called_With_Valid_Credentials_Returns_Ok()
    {
        var app = new IdentityWebApplicationFactory();
        var client = app.CreateClient();

        await client.PostAsJsonAsync("/signup", new
        {
            Email = "test@example.com",
            Password = "Password123!",
            firstName = "Test",
            lastName = "User"
        }, TestContext.Current.CancellationToken);

        var result = await client.PostAsJsonAsync("/login", new
        {
            Email = "test@example.com",
            Password = "Password123!"
        }, TestContext.Current.CancellationToken);

        result.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task When_Login_Is_Called_With_Invalid_Credentials_Returns_BadRequest()
    {
        var app = new IdentityWebApplicationFactory();
        var client = app.CreateClient();

        await client.PostAsJsonAsync("/signup", new
        {
            Email = "test@example.com",
            Password = "Password123!",
            firstName = "Test",
            lastName = "User"
        }, TestContext.Current.CancellationToken);

        var result = await client.PostAsJsonAsync("/login", new
        {
            Email = "test@example.com",
            Password = "Password123!"
        }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    }

    [Fact]
    public async Task When_Login_Is_Called_With_Nonexistent_Email_Returns_BadRequest()
    {
        var app = new IdentityWebApplicationFactory();
        var client = app.CreateClient();

        var result = await client.PostAsJsonAsync("/login", new
        {
            Email = "nonexistent@example.com",
            Password = "Password123!"
        }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    }

    [Fact]
    public async Task When_Login_Is_Called_With_Empty_Fields_Returns_BadRequest()
    {
        var app = new IdentityWebApplicationFactory();
        var client = app.CreateClient();

        var result = await client.PostAsJsonAsync("/login", new
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