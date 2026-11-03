namespace HealthcareStore.Domain.Payments;

public enum PaymentStatus
{
    Pending = 1,
    Captured = 2,
    Expired = 3,
    Cancelled = 4
}