namespace WebApi.Identity.Features.Users.Dto;

public record GetUserResponse (
    string Id, 
    string Email,
    string FirstName,
    string LastName,
    bool IsEmailVerified,
    bool IsLocked,
    bool IsMfaEnabled,
    DateTime CreatedAt,
    DateTime UpdatedAt
);