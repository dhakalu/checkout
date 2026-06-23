namespace Shared.Exceptions;

public class DuplicateRecordException : Exception
{
    public DuplicateRecordException() : base("A duplicate record already exists.") { }

    public DuplicateRecordException(string message) : base(message) { }

    // Recommended: Pass the entity type and the value that caused the conflict
    public DuplicateRecordException(string entityName, string propertyName, object value)
        : base($"{entityName} with {propertyName} '{value}' already exists.") { }

    public DuplicateRecordException(string entityName, params string[] properties)
        : base($"{entityName} with the same {string.Join(" or ", properties)} already exists.") { }
}