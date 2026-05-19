using Shared.DependencyInjection;
using WebApi.Identity.Features.Clients.Data;

namespace WebApi.Identity.Features.Clients.DeleteClient;

public class DeleteClientHandler(ClientRepository clientRepository) : IHandler
{

    private readonly ClientRepository _clientRepository = clientRepository;

    public async Task<bool> HandleAsync(DeleteClientCommand cmd, CancellationToken cancellationToken)
    {
        var count = await _clientRepository.DeleteAsync(cmd.Id, cancellationToken);
        if (count == 0)
        {
            throw new KeyNotFoundException("Client for given id does not exist.");
        }

        return true;
    }
}
