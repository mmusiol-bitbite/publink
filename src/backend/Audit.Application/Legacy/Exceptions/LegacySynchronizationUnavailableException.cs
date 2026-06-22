namespace Audit.Application.Legacy;
public sealed class LegacySynchronizationUnavailableException : LegacySynchronizationException
{
    public LegacySynchronizationUnavailableException()
    {
    }

    public LegacySynchronizationUnavailableException(string message)
        : base(message)
    {
    }

    public LegacySynchronizationUnavailableException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
