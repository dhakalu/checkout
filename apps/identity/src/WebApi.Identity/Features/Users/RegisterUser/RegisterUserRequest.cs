

namespace WebApi.Identity.Features.Users.RegisterUser;

public record RegisterUserRequest(string Password, string Email, string FirstName, string LastName);