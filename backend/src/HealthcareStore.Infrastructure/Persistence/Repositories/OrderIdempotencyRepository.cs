using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Application.Orders.Models;
using HealthcareStore.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace HealthcareStore.Infrastructure.Persistence.Repositories;

public sealed class OrderIdempotencyRepository(
    ApplicationDbContext dbContext) : IOrderIdempotencyRepository
{
    public Task<OrderIdempotencyEntry?> FindAsync(
        Guid userId,
        string idempotencyKey,
        CancellationToken cancellationToken = default)
    {
        return dbContext.OrderIdempotencyRecords
            .AsNoTracking()
            .Where(record =>
                record.UserId == userId &&
                record.IdempotencyKey == idempotencyKey)
            .Select(record => new OrderIdempotencyEntry(
                record.UserId,
                record.IdempotencyKey,
                record.RequestHash,
                record.OrderId,
                record.CreatedAtUtc,
                record.ExpiresAtUtc))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task AddAsync(
        OrderIdempotencyEntry entry,
        CancellationToken cancellationToken = default)
    {
        var record = OrderIdempotencyRecord.Create(
            entry.UserId,
            entry.IdempotencyKey,
            entry.RequestHash,
            entry.OrderId,
            entry.CreatedAtUtc,
            entry.ExpiresAtUtc);

        dbContext.OrderIdempotencyRecords.Add(record);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}