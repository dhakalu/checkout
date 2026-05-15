namespace WebApi.Identity.Features.Auth.Token;

public class PasswordGrantCommand
{
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;

}