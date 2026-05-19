using Shared.DependencyInjection;
using WebApi.Identity.Features.Clients.Data;

namespace WebApi.Identity.Features.Clients.GetClient;

public class GetClientHandler(ClientRepository clientRepository) : IHandler
{

    private readonly ClientRepository _clientRepository = clientRepository;
    public async Task<GetClientResponse> HandleAsync(GetClientQuery query, CancellationToken cancellationToken)
    {
        var client = await _clientRepository.GetByIdAsync(query.Id, cancellationToken)
            ?? throw new KeyNotFoundException("Client with given id does not exist");
        return client;
    }

}
