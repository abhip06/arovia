namespace HealthcareStore.Domain.Payments;

public enum PaymentAttemptStatus
{
    Initiated = 1,
    Pending = 2,
    Authorized = 3,
    Failed = 4,
    Captured = 5,
    Cancelled = 6,
    Expired = 7
}