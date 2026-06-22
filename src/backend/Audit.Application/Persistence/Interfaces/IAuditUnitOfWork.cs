namespace Audit.Application.Persistence;

public interface IAuditUnitOfWork
{
    Task CommitAsync(CancellationToken cancellationToken);
}
