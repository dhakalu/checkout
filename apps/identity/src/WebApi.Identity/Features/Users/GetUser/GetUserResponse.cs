namespace WebApi.Identity.Features.Users.GetUser;

public record GetUserResponse
{
    public string Id { get; init; } = default!;
    public string Email { get; init; } = default!;
    public string FirstName { get; init; } = default!;
    public string LastName { get; init; } = default!;
    public bool IsEmailVerified { get; init; } = default!;
    public bool IsLocked { get; init; } = default!;
    public bool IsMfaEnabled { get; init; } = default!;
    public DateTime CreatedAt { get; init; } = default!;
    public DateTime UpdatedAt { get; init; } = default!;
}