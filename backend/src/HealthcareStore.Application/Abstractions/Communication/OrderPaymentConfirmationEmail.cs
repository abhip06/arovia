namespace HealthcareStore.Application.Abstractions.Communication;

public sealed record OrderPaymentConfirmationEmail(
    string FirstName,
    string OrderNumber,
    string Currency,
    decimal Amount,
    DateTimeOffset PaymentConfirmedAtUtc);