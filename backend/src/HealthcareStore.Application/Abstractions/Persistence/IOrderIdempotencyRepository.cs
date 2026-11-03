using HealthcareStore.Application.Orders.Models;

namespace HealthcareStore.Application.Abstractions.Persistence;

public interface IOrderIdempotencyRepository
{
    Task<OrderIdempotencyEntry?> FindAsync(
        Guid userId,
        string idempotencyKey,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        OrderIdempotencyEntry entry,
        CancellationToken cancellationToken = default);
}