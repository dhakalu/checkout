using System.ComponentModel.DataAnnotations;

namespace WebApi.Identity.Features.Clients.Data;

public class Client
{
    [Key]
    public Guid Id {get; set;} = default!;

    public string Name {get; set;} = default!;

    public string Description {get; set;} = default!;

    public bool IsActive {get; set;}

    public List<ClientSecret> Secrets {get; set;} = [];

}