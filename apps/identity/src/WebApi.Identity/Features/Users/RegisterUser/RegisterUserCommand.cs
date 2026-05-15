namespace WebApi.Identity.Features.Users.RegisterUser;


public record RegisterUserCommand(string Password, string Email, string FirstName, string LastName);