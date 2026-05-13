using Microsoft.AspNetCore.Mvc.Testing;

namespace Tests;

public class ProgramTests
{
    [Fact]
    public void When_Healthcheck_Is_Called_Returns_Ok()
    {
        var app = new WebApplicationFactory<Program>();
        var client = app.CreateClient();

        client.GetAsync("/health").Result.EnsureSuccessStatusCode();


    }
}
