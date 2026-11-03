namespace HealthcareStore.Application.Payments.Models;

public sealed record PaymentHistoryItemDto(
    Guid Id,
    Guid OrderId,
    string Status,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);