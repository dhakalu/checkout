using FluentValidation;
using WebApi.Identity.Features.Auth.Dto;

namespace WebApi.Identity.Features.Auth.Token;

public static class IssueTokenHandler
{
    public static async Task HandleAsync(AuthorizeRequest request, IssueTokenRequestValidator requestValidator, IssueTokenCommand cmd, CancellationToken cancellationToken)
    {
        requestValidator.ValidateAndThrow(request);
        await cmd.Execute(request, cancellationToken);
    }
}