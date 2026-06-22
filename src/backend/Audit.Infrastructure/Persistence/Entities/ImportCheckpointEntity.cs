namespace Audit.Infrastructure.Persistence.Entities;

public sealed class ImportCheckpointEntity
{
    public required string Source { get; set; }

    public long LastSourceEventId { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
