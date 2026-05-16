using System.Text.Json.Serialization;

namespace WebApi.Identity.Features.Auth.Token;

public class IssueTokenRequest
{
    public string? Email { get; init; } = default;
    public string? Password { get; init; } = default;

    [JsonPropertyName("grant_type")]

    public string GrantType { get; init; } = "password";

    public string? ClientId { get; init; }

    public string? ClientSecret { get; init; }
}