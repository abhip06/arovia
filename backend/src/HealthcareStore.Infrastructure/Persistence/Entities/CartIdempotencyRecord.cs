namespace HealthcareStore.Infrastructure.Persistence.Entities;

public sealed class CartIdempotencyRecord
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string IdempotencyKey { get; set; } = null!;

    public string RequestHash { get; set; } = null!;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset ExpiresAt { get; set; }
}