using HealthcareStore.Application.Checkout;

namespace HealthcareStore.Application.Abstractions.Persistence;

public interface ICheckoutProductReader
{
    Task<CheckoutData> GetAsync(
        Guid userId,
        string pincode,
        DateTimeOffset now,
        CancellationToken cancellationToken = default);
}