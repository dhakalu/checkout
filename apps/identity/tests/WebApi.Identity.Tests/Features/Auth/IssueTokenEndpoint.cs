using System.Net;
using Microsoft.AspNetCore.Mvc;
using WebApi.Identity.Features.Auth.Token;
using WebApi.Identity.Features.Users.RegisterUser;

namespace WebApi.Identity.Tests.Features.Auth;


public class IssueTokenEndpointTests

{
    #region token endpoints
    [Fact]
    public async Task When_IssueToken_Is_Called_With_Valid_Credentials_Returns_Ok()
    {
        var app = new IdentityWebApplicationFactory();
        var client = app.CreateClient();

        var createResult = await client.PostAsJsonAsync(RegisterUserEndpoint.Path, new
        {
            Email = "test_authorize_success@example.com",
            Password = "Password123!",
            firstName = "Test",
            lastName = "User"
        }, TestContext.Current.CancellationToken);
        createResult.EnsureSuccessStatusCode();

        var result = await client.PostAsJsonAsync(IssueTokenEndpoint.BasePath, new
        {
            Email = "test_authorize_success@example.com",
            Password = "Password123!",
            GrantType = "password"
        }, TestContext.Current.CancellationToken);

        result.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task When_IssueToken_Is_Called_With_Invalid_Credentials_Returns_Unauthorized()
    {
        var app = new IdentityWebApplicationFactory();
        var client = app.CreateClient();
 
        var createResult = await client.PostAsJsonAsync(RegisterUserEndpoint.Path, new
        {
            Email = "test_authorize_invalid_password@example.com",
            Password = "Password123!",
            firstName = "Test",
            lastName = "User"
        }, TestContext.Current.CancellationToken);
        createResult.EnsureSuccessStatusCode();

        var result = await client.PostAsJsonAsync(IssueTokenEndpoint.BasePath, new
        {
            Email = "test_authorize_invalid_password@example.com",
            Password = "InvalidPassword!",
            GrantType = "password"
        }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
    }

    [Fact]
    public async Task When_IssueToken_Is_Called_With_Nonexistent_Email_Returns_Unauthorized()
    {
        var app = new IdentityWebApplicationFactory();
        var client = app.CreateClient();

        var result = await client.PostAsJsonAsync(IssueTokenEndpoint.BasePath, new
        {
            Email = "testuser-does-not-exist@example.com",
            Password = "Password123!",
            GrantType = "password"
        }, TestContext.Current.CancellationToken);
        await result.Content.ReadFromJsonAsync<ValidationProblemDetails>(cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
   }

    [Fact]
    public async Task When_IssueToken_IsCalledWith_EmptyEmailAndPassword_WhenGrantTypeIsPassword_Returns_BadRequest()
    {
        var app = new IdentityWebApplicationFactory();
        var client = app.CreateClient();

        var result = await client.PostAsJsonAsync(IssueTokenEndpoint.BasePath, new
        {
            Email = "",
            Password = "",
            GrantType = "password"
        }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);

        var validationErrors = await result.Content.ReadFromJsonAsync<ValidationProblemDetails>(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(validationErrors);
        Assert.Contains("email", validationErrors.Errors.Keys);
        Assert.Contains("password", validationErrors.Errors.Keys);
        Assert.Contains("Email is required when grant type is 'password'.", validationErrors.Errors["email"]);
        Assert.Contains("Password is required when grant type is 'password'.", validationErrors.Errors["password"]);
    }

    [Fact]
     public async Task When_IssueToken_Is_EmptyEmailAndPassword_WhenScopeIsClientCredentials_Returns_BadRequest()
    {
        var app = new IdentityWebApplicationFactory();
        var client = app.CreateClient();

        var result = await client.PostAsJsonAsync(IssueTokenEndpoint.BasePath, new
        IssueTokenRequest{
            Email = "",
            Password = "",
            GrantType = "client_credentials"
        }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);

        var validationErrors = await result.Content.ReadFromJsonAsync<ValidationProblemDetails>(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(validationErrors);
        Assert.DoesNotContain("email", validationErrors.Errors.Keys);
        Assert.DoesNotContain("password", validationErrors.Errors.Keys);
        Assert.Contains("clientId", validationErrors.Errors.Keys);
        Assert.Contains("clientSecret", validationErrors.Errors.Keys);
         Assert.Contains("Client id is required when grant type is 'client_credentials'.", validationErrors.Errors["clientId"]);
        Assert.Contains("Client secret is required when grant type is 'client_credentials'.", validationErrors.Errors["clientSecret"]);

    }
    #endregion
}