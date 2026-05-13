using Microsoft.AspNetCore.Mvc.Testing;

using {{cookiecutter.project_namespace}};

namespace {{cookiecutter.tests_namespace}};

public class {{cookiecutter.__clean_name}}WebApplicationFactory : WebApplicationFactory<Program>
{
    protected override IHost CreateHost(IHostBuilder builder)
    {
        // Add any additional configuration or services here if needed

        return base.CreateHost(builder);
    }
}