namespace WebApi.Identity.Features.Auth.Token;

public class IssueTokenCommand
{
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;
}