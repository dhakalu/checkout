namespace Shared.Exceptions;

public class RecordNotFoundException : Exception
{
    public RecordNotFoundException() : base("Record you are looking for is missing.") { }

    public RecordNotFoundException(string message)
        : base(message) { }

}