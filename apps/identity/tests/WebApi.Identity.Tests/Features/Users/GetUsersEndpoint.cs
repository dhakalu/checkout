using System.Net;
using Microsoft.AspNetCore.Mvc;
using WebApi.Identity.Features.Users;
using WebApi.Identity.Features.Users.Dto;

namespace WebApi.Identity.Tests.Features.Users;


public partial class UsersEndpointsTest
{
    [Fact]
    public async Task When_GetUser_Is_Called_WithCorrectId_Returns_Ok()
    {
        var app = new IdentityWebApplicationFactory();
        var client = app.CreateClient();

        var registrationRequest = new {
            Email="get_users_correct_id@example.com",
            Password="Password123!",
            FirstName="Test",
            LastName="Test"
        };
        var registerNewUserResult = await client.PostAsJsonAsync(UsersEndpoints.BasePath, registrationRequest, TestContext.Current.CancellationToken);

        registerNewUserResult.EnsureSuccessStatusCode();

        var newUserRegistrationResponse = await registerNewUserResult.Content.ReadFromJsonAsync<RegisterUserResponse>(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(newUserRegistrationResponse);
        var getUserDetailsResult = await client.GetAsync($"{UsersEndpoints.BasePath}/${newUserRegistrationResponse.Id}", TestContext.Current.CancellationToken);
        getUserDetailsResult.EnsureSuccessStatusCode();

        var userDetails = await getUserDetailsResult.Content.ReadFromJsonAsync<GetUserResponse>(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(userDetails);
        Assert.Equal(registrationRequest.Email, userDetails.Email);
        Assert.Equal(registrationRequest.LastName, userDetails.LastName);
        Assert.Equal(registrationRequest.FirstName, userDetails.FirstName);
        Assert.False(userDetails.IsEmailVerified);
        Assert.False(userDetails.IsLocked);
        Assert.False(userDetails.IsMfaEnabled);
    }

    [Fact]
    public async Task When_GetUser_Is_Called_WithInvalidId_Returns_Notfound()
    {
        var webApp = new IdentityWebApplicationFactory();
        var client = webApp.CreateClient();
        var randomGuid = Guid.NewGuid().ToString();
        var result = await client.GetAsync($"{UsersEndpoints.BasePath}/${randomGuid}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
    }

    
}