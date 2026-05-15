namespace WebApi.Identity.Features.Auth.Token;

using FluentValidation;
using WebApi.Identity;

public class IssueTokenEndpoint: IEndpoint
{

    public const string BasePath = "/authorize";
    public void MapEndpoints(IEndpointRouteBuilder route)
    {
        route.MapPost("/authorize", HandleAsync);
    }

    private static async Task HandleAsync(IssueTokenRequest request, IssueTokenRequestValidator requestValidator, IssueTokenHandler handler, CancellationToken cancellationToken)
    {
        requestValidator.ValidateAndThrow(request);
        var cmd = new IssueTokenCommand
        {
            Email = request.Email,
            Password = request.Password
        };
        await handler.Execute(cmd, cancellationToken);
    }
}