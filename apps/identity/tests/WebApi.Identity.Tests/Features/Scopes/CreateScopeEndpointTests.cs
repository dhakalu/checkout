using System.Net;
using Microsoft.AspNetCore.Mvc;
using WebApi.Identity.Features.Scopes.CreateScope;
using WebApi.Identity.Utilities;

namespace WebApi.Identity.Tests.Features.Scopes;

public class CreateScopeEndpointTests(IdentityWebApplicationFactory app) : IClassFixture<IdentityWebApplicationFactory>
{

    private readonly HttpClient _client = app.CreateClient();

    [Fact]
    public async Task CreateScope_ValidInput_ReturnsOk()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var result = await _client.PostAsJsonAsync("/scopes", new
        CreateScopeRequest
        {
            Key = "scopes:create",
            Name = "Create Scope",
            Description = "Allows user to create scopes"
        }, cancellationToken);
        Assert.Equal(HttpStatusCode.Created, result.StatusCode);
    }

    [Fact]
    public async Task CreateScope_InvalidInput_ReturnsBadRequest()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var result = await _client.PostAsJsonAsync("/scopes", new
        CreateScopeRequest
        {
            Key = "",
            Name = "",
            Description = ""
        }, cancellationToken);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        var validationErrors = await result.Content.ReadFromJsonAsync<ValidationProblemDetails>(cancellationToken);
        Assert.NotNull(validationErrors);
        Assert.Contains("key", validationErrors.Errors.Keys);
        Assert.Contains("name", validationErrors.Errors.Keys);
        Assert.Contains("description", validationErrors.Errors.Keys);
        Assert.Contains("Name is required.", validationErrors.Errors["name"]);
        Assert.Contains("Key is required.", validationErrors.Errors["key"]);
        Assert.Contains("Description is required.", validationErrors.Errors["description"]);
    }


    [Fact]
    public async Task CreateScope_MaxLengthsInput_ReturnsBadRequest()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var result = await _client.PostAsJsonAsync("/scopes", new
        CreateScopeRequest
        {
            Key = FakerUtil.GetRandomStringWithLength(51),
            Name = FakerUtil.GetRandomStringWithLength(101),
            Description = FakerUtil.GetRandomStringWithLength(501)
        }, cancellationToken);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        var validationErrors = await result.Content.ReadFromJsonAsync<ValidationProblemDetails>(cancellationToken);
        Assert.NotNull(validationErrors);
        Assert.Contains("key", validationErrors.Errors.Keys);
        Assert.Contains("name", validationErrors.Errors.Keys);
        Assert.Contains("description", validationErrors.Errors.Keys);
        Assert.Contains("Name must be shorter than 100 characters.", validationErrors.Errors["name"]);
        Assert.Contains("Key must be shorter than 50 characters.", validationErrors.Errors["key"]);
        Assert.Contains("Description must be shorter than 500 characters.", validationErrors.Errors["description"]);
    }

    [Fact]

    public async Task CreateScope_DuplicateKey_ReturnsConflict()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        var create1 = await _client.PostAsJsonAsync(CreateScopeEndpoint.Path, new CreateScopeRequest
        {
            Key = "scope:duplicate",
            Name = "Test",
            Description = "Test"
        }, cancellationToken);
        Assert.Equal(HttpStatusCode.Created, create1.StatusCode);

        var create2 = await _client.PostAsJsonAsync(CreateScopeEndpoint.Path, new CreateScopeRequest
        {
            Key = "scope:duplicate",
            Name = "Test",
            Description = "Test"
        }, cancellationToken);
        Assert.Equal(HttpStatusCode.Conflict, create2.StatusCode);

        var create2Response = await create2.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken);
        Assert.NotNull(create2Response);
        Assert.Equal("Scope with given key already exists.", create2Response.Detail);
    }

}
