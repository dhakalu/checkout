using Microsoft.AspNetCore.Mvc.Testing;

using Orders.WebApi;

namespace Orders.WebApi.Tests;

public class OrdersWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override IHost CreateHost(IHostBuilder builder)
    {
        // Add any additional configuration or services here if needed

        return base.CreateHost(builder);
    }
}