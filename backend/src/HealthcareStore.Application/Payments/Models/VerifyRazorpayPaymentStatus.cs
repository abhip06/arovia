namespace HealthcareStore.Application.Payments.Models;

public enum VerifyRazorpayPaymentStatus
{
    Captured = 1,
    AlreadyCaptured = 2,
    PaymentPending = 3,
    InvalidSignature = 4,
    PaymentNotFound = 5,
    OrderNotFound = 6,
    InvalidPayment = 7,
    OrderNotPayable = 8,
    ReconciliationRequired = 9,
    PaymentFailed = 10
}