using HealthcareStore.Domain.Common;

namespace HealthcareStore.Domain.Auth;

public sealed class EmailVerificationToken : Entity
{
    private EmailVerificationToken()
    {
        // Required by EF Core.
    }

    private EmailVerificationToken(
        Guid userId,
        string tokenHash,
        DateTimeOffset expiresAt)
    {
        UserId = userId;
        TokenHash = tokenHash;
        ExpiresAt = expiresAt;

        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid UserId { get; private set; }

    public string TokenHash { get; private set; } = null!;

    public DateTimeOffset ExpiresAt { get; private set; }

    public DateTimeOffset? RevokedAt { get; private set; }

    public DateTimeOffset? UsedAt { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public static EmailVerificationToken Create(
        Guid userId,
        string tokenHash,
        DateTimeOffset expiresAt)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException(
                "User ID cannot be empty.",
                nameof(userId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(
            tokenHash,
            nameof(tokenHash));

        if (expiresAt <= DateTimeOffset.UtcNow)
        {
            throw new ArgumentException(
                "Token expiration must be in the future.",
                nameof(expiresAt));
        }

        return new EmailVerificationToken(
            userId,
            tokenHash,
            expiresAt);
    }

    public void MarkUsed()
    {
        if (UsedAt.HasValue)
        {
            return;
        }

        UsedAt = DateTimeOffset.UtcNow;
    }

    public void Revoke()
    {
        if (UsedAt.HasValue || RevokedAt.HasValue)
        {
            return;
        }

        RevokedAt = DateTimeOffset.UtcNow;
    }
}