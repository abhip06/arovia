namespace HealthcareStore.Application.Payments.Models;

public sealed record VerifyRazorpayPaymentRequest(
    Guid PaymentAttemptId,
    string ProviderOrderId,
    string ProviderPaymentId,
    string Signature);