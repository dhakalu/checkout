using FluentValidation;
using WebApi.Identity.Features.Authorize.Dto;
using WebApi.Identity.Features.Authorize.Validators;

namespace WebApi.Identity.Features.Authorize;

public static class AuthorizeHandlers
{
    public static async Task HandleAuthorizeAsync(AuthorizeRequest request, AuthorizeRequestValidator requestValidator, AuthorizeService service, CancellationToken cancellationToken)
    {
        requestValidator.ValidateAndThrow(request);
        await service.Authorize(request, cancellationToken);
    }
}