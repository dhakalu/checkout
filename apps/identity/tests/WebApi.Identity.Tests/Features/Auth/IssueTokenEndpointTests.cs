using System.Net;
using Microsoft.AspNetCore.Mvc;
using WebApi.Identity.Features.Auth.Token;
using WebApi.Identity.Features.Users.RegisterUser;

namespace WebApi.Identity.Tests.Features.Auth;


public class IssueTokenEndpointTests

{
    #region password grant type
    [Fact]
    public async Task IssueToken_PasswordGrantType_ValidCredentials_ReturnsOk()
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
    public async Task IssueToken_PasswordGrantType_InvalidCredentials_ReturnsUnauthorized()
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
    public async Task IssueToken_PasswordGrantType_NonexistentEmail_ReturnsUnauthorized()
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
    public async Task IssueToken_PasswordGrantType_EmptyEmailAndPassword_ReturnsBadRequest()
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
    #endregion
    #region client credentials
    [Fact]
    public async Task IssueToken_ClientCredentialsGrantType_EmptyEmailAndPassword_DoesNotIncludeEmailPasswordErrors()
    {
        var app = new IdentityWebApplicationFactory();
        var client = app.CreateClient();

        var result = await client.PostAsJsonAsync(IssueTokenEndpoint.BasePath, new
        IssueTokenRequest
        {
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