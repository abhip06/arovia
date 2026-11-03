using HealthcareStore.Application.Abstractions.Persistence;
using Microsoft.EntityFrameworkCore.Storage;

namespace HealthcareStore.Infrastructure.Persistence;

public sealed class EfTransactionManager(
    ApplicationDbContext dbContext) : ITransactionManager
{
    private IDbContextTransaction? _transaction;

    public async Task BeginAsync(
        CancellationToken cancellationToken)
    {
        if (_transaction is not null)
        {
            throw new InvalidOperationException(
                "A transaction is already active.");
        }

        _transaction =
            await dbContext.Database.BeginTransactionAsync(
                cancellationToken);
    }

    public async Task CommitAsync(
        CancellationToken cancellationToken)
    {
        if (_transaction is null)
        {
            throw new InvalidOperationException(
                "No active transaction exists.");
        }

        await _transaction.CommitAsync(
            cancellationToken);

        await _transaction.DisposeAsync();

        _transaction = null;
    }

    public async Task RollbackAsync(
        CancellationToken cancellationToken)
    {
        if (_transaction is null)
        {
            return;
        }

        await _transaction.RollbackAsync(
            cancellationToken);

        await _transaction.DisposeAsync();

        _transaction = null;

        dbContext.ChangeTracker.Clear();
    }
}