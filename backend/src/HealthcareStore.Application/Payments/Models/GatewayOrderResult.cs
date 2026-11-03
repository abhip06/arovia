namespace HealthcareStore.Application.Payments.Models;

public sealed record GatewayOrderResult(
    string Id,
    long AmountSubunits,
    string Currency,
    string? Receipt,
    string Status,
    DateTimeOffset CreatedAtUtc);