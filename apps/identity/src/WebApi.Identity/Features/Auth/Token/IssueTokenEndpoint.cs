namespace WebApi.Identity.Features.Auth.Token;

using FluentValidation;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Server.AspNetCore;
using OpenIddict.Abstractions;
using WebApi.Identity;
using System.Security.Claims;
using Shared.Annotations;

public class IssueTokenEndpoint : IEndpoint
{

    public const string BasePath = "/authorize";
    public void MapEndpoints(IEndpointRouteBuilder route)
    {
        route.MapPost("/connect/token", HandleOpenIddict);
    }

    public static async Task<IResult> HandleOpenIddict(HttpContext context, PasswordGrantHandler handler, CancellationToken cancellationToken)
    {
        // OpenIddict extracts the OAuth request details for you
        var request = OpenIddictServerAspNetCoreHelpers.GetOpenIddictServerRequest(context);
        if (request == null)
        {
            return Results.BadRequest("The OpenID Connect request cannot be retrieved.");
        }
        if (request.IsPasswordGrantType())
        {

            var cmd = new PasswordGrantCommand
            {
                Email = request.Username!,
                Password = request.Password!
            };
            var user = await handler.Execute(cmd, cancellationToken);


            var identity = new ClaimsIdentity(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            identity.AddClaim(OpenIddictConstants.Claims.Subject, user.Id);
            identity.AddClaim(OpenIddictConstants.Claims.Email, user.Email);

            identity.AddClaim("permission", "reports:view");
            identity.AddClaim("permission", "reports:create");

            var principal = new ClaimsPrincipal(identity);

            return Results.SignIn(principal);
        }
        return Results.Forbid();
    }

    private static async Task<IResult> HandleAsync(IssueTokenRequest request, IssueTokenRequestValidator requestValidator, PasswordGrantHandler handler, CancellationToken cancellationToken)
    {
        requestValidator.ValidateAndThrow(request);
        switch (request.GrantType)
        {
            case "password":
                var cmd = new PasswordGrantCommand
                {
                    Email = request.Email!,
                    Password = request.Password!
                };
                var tokens = await handler.Execute(cmd, cancellationToken);
                return Results.Ok(tokens);
            case "client_credentials":
                return Results.Forbid();
        }
        return Results.Forbid();
    }
}