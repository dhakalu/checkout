using Microsoft.AspNetCore.Mvc.Testing;

namespace Inventory.WebApi.Tests.Tests;

public class ProgramTests(InventoryWebApplicationFactory app) : IClassFixture<InventoryWebApplicationFactory>
{
    private readonly HttpClient _client = app.CreateClient();
    [Fact]
    public async Task When_Healthcheck_Is_Called_Returns_Ok()
    {

        var result = await _client.GetAsync("/health", TestContext.Current.CancellationToken);
        result.EnsureSuccessStatusCode();

    }
}