namespace Shared.Exceptions;

public class MissingConfigurationException : Exception
{
    public MissingConfigurationException() : base("Required configuration is missing.") { }

    public MissingConfigurationException(string configurationName)
        : base($"Required configuration '{configurationName}' is missing") { }

}