namespace WebApi.Identity.Features.Users.Data;


public class User
{
    public string Id { get; set; } = default!;
    public string Email { get; set; } = default!;
    public bool IsEmailVerified { get; set; } = false;

    public bool IsLocked { get; set; } = false;

    public bool IsMfaEnabled { get; set; } = false;
    public string PasswordHash { get; set; } = default!;
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;

    public DateTime CreatedAt { get; set; } = default!;

    public DateTime UpdatedAt { get; set; } = default!;
}