namespace HealthcareStore.Application.Payments.Models;

public sealed record GatewayPaymentResult(
    string Id,
    string? OrderId,
    long AmountSubunits,
    string Currency,
    string Status,
    string Method,
    bool Captured,
    DateTimeOffset CreatedAtUtc);