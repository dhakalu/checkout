namespace WebApi.Identity.Features.Auth.Dto;

public record AuthorizeResponse(string IdToken, string AccessToken);