namespace HealthcareStore.Domain.Payments;

public enum PaymentEventType
{
    PaymentCreated = 1,
    PaymentAttemptCreated = 2,
    RazorpayOrderCreated = 3,
    CheckoutCallbackReceived = 4,
    PaymentAuthorized = 5,
    PaymentFailed = 6,
    PaymentCaptured = 7,
    PaymentAttemptCancelled = 8,
    PaymentAttemptExpired = 9,
    PaymentSessionExpired = 10,
    PaymentCancelled = 11,
    OrderConfirmed = 12,
    DuplicatePaymentDetected = 13,
    ReconciliationRequired = 14
}