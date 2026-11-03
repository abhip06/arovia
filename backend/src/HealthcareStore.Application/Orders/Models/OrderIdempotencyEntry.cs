namespace HealthcareStore.Application.Orders.Models;

public sealed record OrderIdempotencyEntry(
    Guid UserId,
    string IdempotencyKey,
    string RequestHash,
    Guid OrderId,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset ExpiresAtUtc);