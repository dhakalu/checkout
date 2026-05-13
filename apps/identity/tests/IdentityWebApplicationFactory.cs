
using Microsoft.AspNetCore.Mvc.Testing;
public class IdentityWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override IHost CreateHost(IHostBuilder builder)
    {
        // Add any additional configuration or services here if needed

        return base.CreateHost(builder);
    }
}