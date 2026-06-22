using Audit.Contracts;

namespace Audit.Application.Legacy;

public interface ILegacySynchronizationRequester
{
    Task RequestAsync(
        RequestLegacySynchronizationV1 command,
        CancellationToken cancellationToken);
}
