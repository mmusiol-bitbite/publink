namespace Audit.Application.Queries;

public sealed class ContractStoreUnavailableException : ContractStoreException
{
    public ContractStoreUnavailableException()
    {
    }

    public ContractStoreUnavailableException(string message)
        : base(message)
    {
    }

    public ContractStoreUnavailableException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
