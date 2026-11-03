using HealthcareStore.Application.Helpers;

namespace HealthcareStore.Infrastructure.Persistence.Repositories;

public sealed class UnconfiguredOrderPrescriptionVerificationReader
    : IOrderPrescriptionVerificationReader
{
    public Task<bool> AreAllRequiredPrescriptionsVerifiedAsync(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(false);
    }
}