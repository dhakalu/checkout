namespace WebApi.Identity.Features.Auth.Token;

using FluentValidation;
using WebApi.Identity;

public class IssueTokenEndpoint : IEndpoint
{

    public const string BasePath = "/authorize";
    public void MapEndpoints(IEndpointRouteBuilder route)
    {
        route.MapPost("/authorize", HandleAsync);
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