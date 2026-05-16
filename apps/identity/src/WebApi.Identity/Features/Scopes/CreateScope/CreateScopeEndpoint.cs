using FluentValidation;

namespace WebApi.Identity.Features.Scopes.CreateScope;

public class CreateScopeEndpoint : IEndpoint
{
    public const string Path = "/scopes";
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost(Path, HandleAsync);
    }

    private async Task<IResult> HandleAsync(CreateScopeRequest request,
        CreateScopeValidator validator,
        CreateScopeHandler handler, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);
        var cmd = new CreateScopeCommand(request.Key, request.Name, request.Description);
        await handler.HandleAsync(cmd, cancellationToken);
        return Results.Created();
    }
}
