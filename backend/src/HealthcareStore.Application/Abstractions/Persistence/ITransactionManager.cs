namespace HealthcareStore.Application.Abstractions.Persistence;

public interface ITransactionManager
{
    Task BeginAsync(
        CancellationToken cancellationToken);

    Task CommitAsync(
        CancellationToken cancellationToken);

    Task RollbackAsync(
        CancellationToken cancellationToken);
}