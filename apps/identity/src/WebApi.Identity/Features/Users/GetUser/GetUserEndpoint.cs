using System.Reflection.Metadata;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Identity.Features.Users.GetUser;


public class GetUserEndpoint: IEndpoint
{
    public const string Path = "/users";

    public void MapEndpoints(IEndpointRouteBuilder group)
    {

        group.MapGet("/users/{id}", HandleRegisterUserAsync)
        .WithName("GetUser");
    }

    private static async Task<GetUserResponse> HandleRegisterUserAsync([FromRoute]Guid id, 
    GetUserHandler handler, CancellationToken cancellationToken)
    {
        var cmd = new GetUserQuery(id);
        return await handler.HandleAsync(cmd, cancellationToken);
    }
}