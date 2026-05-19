using Shared.DependencyInjection;
using WebApi.Identity.Features.Users.Data;

namespace WebApi.Identity.Features.Users.GetUser;

public class GetUserHandler(UserRepository identityRepository) : IHandler
{

    private readonly UserRepository _identityRepository = identityRepository;
    public async Task<GetUserResponse> HandleAsync(GetUserQuery query, CancellationToken cancellationToken)
    {
        var identity = await _identityRepository.GetByIdAsync(query.Id, cancellationToken)
        ?? throw new KeyNotFoundException("User does not exist.");

        return new GetUserResponse
        {
            Id = identity.Id,
            Email = identity.Email,
            LastName = identity.LastName,
            FirstName = identity.FirstName,
            IsEmailVerified = identity.IsEmailVerified,
            IsMfaEnabled = identity.IsMfaEnabled,
            IsLocked = identity.IsLocked,
            CreatedAt = identity.CreatedAt,
            UpdatedAt = identity.UpdatedAt
        };

    }
}