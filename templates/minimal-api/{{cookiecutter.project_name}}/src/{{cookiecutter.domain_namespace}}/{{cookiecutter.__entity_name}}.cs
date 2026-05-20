namespace {{cookiecutter.domain_namespace}};

public class {{cookiecutter.__entity_name}}
{
    public Guid Id {get; init; }

    public string Name {get; init; } = default!;
}