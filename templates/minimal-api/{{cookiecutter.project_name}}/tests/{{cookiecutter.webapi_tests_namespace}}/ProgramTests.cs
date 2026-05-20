using Microsoft.AspNetCore.Mvc.Testing;

namespace {{cookiecutter.webapi_tests_namespace}}.Tests;

public class ProgramTests
{
    [Fact]
    public async Task When_Healthcheck_Is_Called_Returns_Ok()
    {
        var app = new {{cookiecutter.__clean_name}}WebApplicationFactory();
        var client = app.CreateClient();

        var result = await client.GetAsync("/health", TestContext.Current.CancellationToken);
        result.EnsureSuccessStatusCode();

    }
}
