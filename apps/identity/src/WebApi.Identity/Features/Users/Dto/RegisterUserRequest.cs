

namespace WebApi.Identity.Features.Users.Dto;

public record RegisterUserRequest(string Password, string Email, string FirstName, string LastName);