using HealthcareStore.Domain.Orders;

namespace HealthcareStore.Application.Abstractions.Persistence;

public enum OrderInventoryReservationResult
{
    Reserved = 1,
    InsufficientStock = 2
}

public interface IOrderInventoryReservationService
{
    Task<OrderInventoryReservationResult> ReserveForOrderAsync(
        Order order,
        string pincode,
        DateOnly today,
        CancellationToken cancellationToken = default);

    Task ReleaseForOrderAsync(
        Order order,
        DateTimeOffset utcNow,
        CancellationToken cancellationToken = default);

    Task FulfillForOrderAsync(
        Order order,
        Guid performedByUserId,
        DateTimeOffset utcNow,
        CancellationToken cancellationToken = default);
}