

namespace WebApi.Identity.Features.Signup.Dto;

public record SignUpRequest(string Password, string Email, string FirstName, string LastName);