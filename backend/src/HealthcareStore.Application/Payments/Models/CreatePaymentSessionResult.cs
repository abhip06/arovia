namespace HealthcareStore.Application.Payments.Models;

public enum CreatePaymentSessionStatus
{
    Created = 1,
    Replayed = 2,
    OrderNotFound = 3,
    OrderNotPayable = 4,
    PaymentExpired = 5,
    PaymentAlreadyCaptured = 6,
    IdempotencyConflict = 7,
    PaymentAttemptInProgress = 8,
    GatewayUnavailable = 9,
    ReconciliationRequired = 10
}

public sealed record PaymentSessionModel(
    Guid PaymentId,
    Guid PaymentAttemptId,
    string KeyId,
    string ProviderOrderId,
    long AmountSubunits,
    string Currency,
    DateTimeOffset ExpiresAtUtc);

public sealed record CreatePaymentSessionResult(
    CreatePaymentSessionStatus Status,
    PaymentSessionModel? Session = null);