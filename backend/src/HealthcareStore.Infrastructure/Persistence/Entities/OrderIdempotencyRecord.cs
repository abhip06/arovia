namespace HealthcareStore.Infrastructure.Persistence.Entities;

public sealed class OrderIdempotencyRecord
{
    private OrderIdempotencyRecord()
    {
    }

    public Guid UserId { get; private set; }

    public string IdempotencyKey { get; private set; } = null!;
    public string RequestHash { get; private set; } = null!;

    public Guid OrderId { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset ExpiresAtUtc { get; private set; }

    public static OrderIdempotencyRecord Create(
        Guid userId,
        string idempotencyKey,
        string requestHash,
        Guid orderId,
        DateTimeOffset createdAtUtc,
        DateTimeOffset expiresAtUtc)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException(
                "User ID is required.",
                nameof(userId));
        }

        if (orderId == Guid.Empty)
        {
            throw new ArgumentException(
                "Order ID is required.",
                nameof(orderId));
        }

        var normalizedKey = Required(
            idempotencyKey,
            nameof(idempotencyKey));

        if (normalizedKey.Length > 128)
        {
            throw new ArgumentException(
                "Idempotency key cannot exceed 128 characters.",
                nameof(idempotencyKey));
        }

        var normalizedHash = Required(
            requestHash,
            nameof(requestHash));

        if (normalizedHash.Length != 64 ||
            !normalizedHash.All(Uri.IsHexDigit))
        {
            throw new ArgumentException(
                "Request hash must be a SHA-256 hexadecimal string.",
                nameof(requestHash));
        }

        EnsureUtc(createdAtUtc, nameof(createdAtUtc));
        EnsureUtc(expiresAtUtc, nameof(expiresAtUtc));

        if (expiresAtUtc <= createdAtUtc)
        {
            throw new ArgumentException(
                "Expiration must be later than creation.",
                nameof(expiresAtUtc));
        }

        return new OrderIdempotencyRecord
        {
            UserId = userId,
            IdempotencyKey = normalizedKey,
            RequestHash = normalizedHash.ToLowerInvariant(),
            OrderId = orderId,
            CreatedAtUtc = createdAtUtc,
            ExpiresAtUtc = expiresAtUtc
        };
    }

    private static string Required(
        string value,
        string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                "Value cannot be empty.",
                parameterName);
        }

        return value.Trim();
    }

    private static void EnsureUtc(
        DateTimeOffset value,
        string parameterName)
    {
        if (value.Offset != TimeSpan.Zero)
        {
            throw new ArgumentException(
                "Timestamp must use UTC.",
                parameterName);
        }
    }
}