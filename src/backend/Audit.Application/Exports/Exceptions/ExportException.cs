namespace Audit.Application.Exports;

public abstract class ExportException : Exception
{
    protected ExportException()
    {
    }

    protected ExportException(string message) : base(message)
    {
    }

    protected ExportException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
