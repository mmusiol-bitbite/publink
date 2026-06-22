namespace Audit.Application.Queries;

public abstract class ContractStoreException : Exception
{
    protected ContractStoreException()
    {
    }
    protected ContractStoreException(string message)
        : base(message)
    {
    }
    protected ContractStoreException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
