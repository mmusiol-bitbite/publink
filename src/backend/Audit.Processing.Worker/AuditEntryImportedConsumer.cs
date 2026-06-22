using Audit.Application.Persistence;
using Audit.Contracts;
using MassTransit;

namespace Audit.Processing.Worker;

internal sealed partial class AuditEntryImportedConsumer(
    ICanonicalAuditEventPersister eventPersister,
    IContractProjectionPersister projectionPersister,
    IAuditUnitOfWork unitOfWork,
    ILogger<AuditEntryImportedConsumer> logger)
    : IConsumer<AuditEntryImportedV1>
{
    public async Task Consume(ConsumeContext<AuditEntryImportedV1> context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var appendResult = await eventPersister.TryAppendAsync(
            context.Message,
            context.CancellationToken);

        if (appendResult == AppendAuditEventResult.Duplicate)
        {
            LogDuplicate(
                logger,
                context.Message.Source,
                context.Message.SourceEventId);
            return;
        }

        await projectionPersister.ApplyAsync(context.Message, context.CancellationToken);
        await unitOfWork.CommitAsync(context.CancellationToken);

        LogProjected(
            logger,
            context.Message.Source,
            context.Message.SourceEventId,
            context.Message.SourceSequence);
    }

    [LoggerMessage(
        EventId = 2001,
        Level = LogLevel.Information,
        Message = "Ignoring duplicate event {Source}/{SourceEventId}")]
    private static partial void LogDuplicate(ILogger logger, string source, long sourceEventId);

    [LoggerMessage(
        EventId = 2002,
        Level = LogLevel.Information,
        Message = "Projected event {Source}/{SourceEventId} at sequence {SourceSequence}")]
    private static partial void LogProjected(
        ILogger logger,
        string source,
        long sourceEventId,
        long sourceSequence);
}
