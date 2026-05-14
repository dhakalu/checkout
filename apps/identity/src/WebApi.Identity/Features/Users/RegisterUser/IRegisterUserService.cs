using WebApi.Identity.Features.Users.Data;
using WebApi.Identity.Features.Users.Dto;

namespace WebApi.Identity.Features.Users.RegisterUser;

public interface IRegisterUserService
{
    // Signup registers a new user with the provided details 
    // and returns a SignupResponse containing the new user's ID.
    public Task<RegisterUserResponse> Register(RegisterUserRequest request,
     CancellationToken cancellationToken);
}