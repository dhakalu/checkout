using Microsoft.AspNetCore.Mvc.Testing;

namespace Orders.WebApi.Tests.Tests;

public class ProgramTests
{
    [Fact]
    public async Task When_Healthcheck_Is_Called_Returns_Ok()
    {
        var app = new OrdersWebApplicationFactory();
        var client = app.CreateClient();

        var result = await client.GetAsync("/health", TestContext.Current.CancellationToken);
        result.EnsureSuccessStatusCode();

    }
}
