namespace Audit.Infrastructure.Persistence.Core;

public interface IDatabaseInitializer
{
    Task InitializeAsync(CancellationToken cancellationToken);
}
