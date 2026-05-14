namespace WebApi.Identity.Features.Signup.Data;


public class Identity
{
    public string Id { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;

    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
}