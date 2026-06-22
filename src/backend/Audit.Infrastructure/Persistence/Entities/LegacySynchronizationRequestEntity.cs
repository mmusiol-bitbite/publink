namespace Audit.Infrastructure.Persistence.Entities;

public sealed class LegacySynchronizationRequestEntity
{
    public required string Source { get; set; }

    public Guid CorrelationId { get; set; }

    public DateTimeOffset RequestedAt { get; set; }

    public DateTimeOffset? CompletedAt { get; set; }
}
