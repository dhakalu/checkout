using WebApi.Identity.Features.Scopes.Data;

namespace WebApi.Identity.Features.Clients.Data;

public class ClientScope
{

    public Guid ClientId { get; set; }

    public string ScopeKey { get; set; } = default!;

    public Client Client { get; set; } = default!;
    public Scope Scope { get; set; } = default!;
}
