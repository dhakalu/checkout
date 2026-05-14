using FluentValidation;
using WebApi.Identity.Features.Auth.Dto;
using WebApi.Identity.Features.Auth.Validators;

namespace WebApi.Identity.Features.Auth;

public static class AuthorizeHandlers
{
    public static async Task HandleAuthorizeAsync(AuthorizeRequest request, AuthorizeRequestValidator requestValidator, AuthorizeService service, CancellationToken cancellationToken)
    {
        requestValidator.ValidateAndThrow(request);
        await service.Authorize(request, cancellationToken);
    }
}