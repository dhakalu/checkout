using WebApi.Identity.Features.Clients.Data;

namespace WebApi.Identity.Features.Clients.RegisterClient;

public class RegisterClientHandler(ClientRepository clientRepository): IHandler
{

    private readonly ClientRepository _clientRepository = clientRepository;
    public async Task<Guid> HandleAsync(RegisterClientCommand cmd, CancellationToken cancellationToken)
    {

        var exits = await _clientRepository.ExistsByNameAsync(cmd.Name, cancellationToken);
        if (exits)
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