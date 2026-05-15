using System.ComponentModel.DataAnnotations;

namespace WebApi.Identity.Features.Clients.Data;

public class ClientSecret
{
    [Key]
    public Guid Id {get; set;} = default!;


    public Guid ClientId {get; set;} = default!;

    public string Secret {get; set;} = default!;

    // When was the secret first created?
    public DateTime CreatedAt {get; set;}

    // when was this secret last roated?
    public DateTime RoatedAt {get; set;}

}
