namespace WebApi.Identity.Features.Clients;

public class RegisterClientHandler()
{
    public async Task<bool> HandleAsync(RegisterClientCommand cmd)
    {
        await Task.Delay(100);
        return true;
    }
}