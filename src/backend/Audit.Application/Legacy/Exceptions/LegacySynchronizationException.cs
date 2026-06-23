namespace Audit.Application.Legacy;

public abstract class LegacySynchronizationException : Exception
{
    protected LegacySynchronizationException()
    {
    }
    protected LegacySynchronizationException(string message)
        : base(message)
    {
    }
    protected LegacySynchronizationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

}
