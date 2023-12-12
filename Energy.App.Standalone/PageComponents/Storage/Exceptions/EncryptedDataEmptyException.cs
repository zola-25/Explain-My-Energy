namespace Energy.App.Standalone.PageComponents.Storage.Exceptions;

public class EncryptedDataEmptyException : Exception
{
    public EncryptedDataEmptyException(string message) : base(message)
    {
    }
}
