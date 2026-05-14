namespace WebApi.Identity.Features.Authorize.Dto;

public class AuthorizeRequest
{
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;
}