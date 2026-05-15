using WebApi.Identity.Features.Clients.Data;

namespace WebApi.Identity.Features.Clients;

public class RegisterClientHandler(ClientRepository clientRepository)
{

    private readonly ClientRepository _clientRepository = clientRepository;
    public async Task<bool> HandleAsync(RegisterClientCommand cmd, CancellationToken cancellationToken)
    {

        var existingClient = await _clientRepository.GetByNameAsync(cmd.Name, cancellationToken);
        if (existingClient != null)
        {
            throw new InvalidOperationException("Client with given name already exists");
        }
        var clientSecret = new ClientSecret
        {
            Secret = "test",
        };

        var client = new Client
        {
            Name = cmd.Name,
            Description = cmd.Description,
            IsActive = cmd.IsActive,
            Secrets = [clientSecret]
        };
        return await _clientRepository.SaveAsync(client, cancellationToken);

    }
}