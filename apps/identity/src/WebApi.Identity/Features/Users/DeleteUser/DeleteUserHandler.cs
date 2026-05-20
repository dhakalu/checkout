using Shared.Annotations;
using WebApi.Identity.Features.Users.Data;

namespace WebApi.Identity.Features.Users.DeleteUser;

public class DeleteUserHandler(UserRepository userRepository) : IHandler
{
    private readonly UserRepository _userRepository = userRepository;
    internal Task<bool> HandleAsync(DeleteUserCommand cmd, CancellationToken cancellationToken)
    {
        return _userRepository.DeleteAsync(cmd.Id, cancellationToken);
    }
}
