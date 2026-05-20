using Microsoft.AspNetCore.Mvc;
using Shared.Annotations;

namespace WebApi.Identity.Features.Users.DeleteUser;

public class DeleteUserEndpoint : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapDelete("/users/{id}", HandleAsync);
    }

    private async Task<IResult> HandleAsync([FromRoute] Guid id, DeleteUserHandler handler, CancellationToken cancellationToken)
    {
        var cmd = new DeleteUserCommand(id);
        var isDeleted = await handler.HandleAsync(cmd, cancellationToken);
        if (!isDeleted)
        {
            throw new Exception("Cannot delete");
        }
        return Results.NoContent();
    }
}
