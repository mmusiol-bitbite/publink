using System.Diagnostics.CodeAnalysis;

namespace Audit.Query.Api.Configuration;

[SuppressMessage("Performance", "CA1812", Justification = "Instantiated by the options configuration binder.")]
internal sealed class SynchronizationOptions
{
    public const string SectionName = "Synchronization";

    public string Source { get; init; } = string.Empty;

    public string DeadLetterQueue { get; init; } = string.Empty;

    public TimeSpan HealthyStatusMaxAge { get; init; }

    public string LegacySynchronizationQueueName { get; init; } = "legacy-synchronization";
}
