namespace WebApi.Identity.Features.Scopes.ValidateScopes;

public class ValidateScopesResponse
{

    public List<string> Valid { get; init; } = [];

    public List<string> Invalid { get; init; } = [];

}
