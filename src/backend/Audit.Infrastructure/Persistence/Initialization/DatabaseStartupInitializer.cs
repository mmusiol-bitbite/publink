using System.Diagnostics.CodeAnalysis;
using Audit.Infrastructure.Persistence.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Audit.Infrastructure.Persistence.Initialization;

public sealed partial class DatabaseStartupInitializer(
    IServiceScopeFactory scopeFactory,
    ILogger<DatabaseStartupInitializer> logger)
{
    [SuppressMessage(
        "Design",
        "CA1031:Do not catch general exception types",
        Justification = "The host intentionally retries all database startup failures until shutdown.")]
    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var initializers = scope.ServiceProvider.GetServices<IDatabaseInitializer>();
                foreach (var initializer in initializers)
                {
                    await initializer.InitializeAsync(cancellationToken);
                }

                DatabasesReady(logger);
                return;
            }
            catch (Exception exception)
            {
                DatabasesNotReady(logger, exception);
                await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
            }
        }
    }

    [LoggerMessage(
        EventId = 5001,
        Level = LogLevel.Information,
        Message = "Required databases are ready")]
    private static partial void DatabasesReady(ILogger logger);

    [LoggerMessage(
        EventId = 5002,
        Level = LogLevel.Warning,
        Message = "Required databases are not ready; retrying initialization")]
    private static partial void DatabasesNotReady(ILogger logger, Exception exception);
}
