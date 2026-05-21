using Microsoft.AspNetCore.Mvc.Testing;

namespace {{cookiecutter.webapi_tests_namespace}}.Tests;

public class ProgramTests({{cookiecutter.__clean_name}}WebApplicationFactory app): IClassFixture<{{cookiecutter.__clean_name}}WebApplicationFactory>
{
    private readonly HttpClient _client = app.CreateClient();
    [Fact]
    public async Task When_Healthcheck_Is_Called_Returns_Ok()
    {

        var result = await _client.GetAsync("/health", TestContext.Current.CancellationToken);
        result.EnsureSuccessStatusCode();

    }
}
