namespace WebApi.Identity.Features.Authorize.Dto;

public record AuthorizeResponse(string IdToken, string AccessToken);