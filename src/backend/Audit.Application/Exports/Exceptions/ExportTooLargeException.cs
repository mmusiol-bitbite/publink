namespace Audit.Application.Exports;

public sealed class ExportTooLargeException : ExportException
{
    public ExportTooLargeException()
    {
    }

    public ExportTooLargeException(string message)
        : base(message)
    {
    }

    public ExportTooLargeException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public ExportTooLargeException(int maximumEvents)
        : base($"Export exceeds the limit of {maximumEvents} events.")
    {
        MaximumEvents = maximumEvents;
    }

    public int MaximumEvents { get; }
}
