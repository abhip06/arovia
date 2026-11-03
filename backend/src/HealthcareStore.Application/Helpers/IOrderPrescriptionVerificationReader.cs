namespace HealthcareStore.Application.Helpers;

public interface IOrderPrescriptionVerificationReader
{
    Task<bool> AreAllRequiredPrescriptionsVerifiedAsync(
        Guid orderId,
        CancellationToken cancellationToken = default);
}