using HealthcareStore.Domain.Common;

namespace HealthcareStore.Domain.Auth;

public sealed class UserSession : Entity
{
    private UserSession()
    {
        // Required by EF Core.
    }

    private UserSession(
        Guid userId,
        string refreshTokenHash,
        Guid tokenFamilyId,
        DateTimeOffset expiresAt)
    {
        UserId = userId;
        RefreshTokenHash = refreshTokenHash;
        TokenFamilyId = tokenFamilyId;
        ExpiresAt = expiresAt;

        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid UserId { get; private set; }

    public string RefreshTokenHash { get; private set; } = null!;

    public Guid TokenFamilyId { get; private set; }

    public DateTimeOffset ExpiresAt { get; private set; }

    public DateTimeOffset? LastUsedAt { get; private set; }

    public DateTimeOffset? RevokedAt { get; private set; }

    public Guid? ReplacedBySessionId { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public static UserSession Create(
        Guid userId,
        string refreshTokenHash,
        Guid tokenFamilyId,
        DateTimeOffset expiresAt)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException(
                "User ID cannot be empty.",
                nameof(userId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(
            refreshTokenHash,
            nameof(refreshTokenHash));

        if (tokenFamilyId == Guid.Empty)
        {
            throw new ArgumentException(
                "Token family ID cannot be empty.",
                nameof(tokenFamilyId));
        }

        if (expiresAt <= DateTimeOffset.UtcNow)
        {
            throw new ArgumentException(
                "Session expiration must be in the future.",
                nameof(expiresAt));
        }

        return new UserSession(
            userId,
            refreshTokenHash,
            tokenFamilyId,
            expiresAt);
    }

    public void MarkUsed()
    {
        LastUsedAt = DateTimeOffset.UtcNow;
    }

    public void Revoke()
    {
        if (RevokedAt.HasValue)
        {
            return;
        }

        RevokedAt = DateTimeOffset.UtcNow;
    }

    public void MarkReplacedBy(Guid replacementSessionId)
    {
        if (replacementSessionId == Guid.Empty)
        {
            throw new ArgumentException(
                "Replacement session ID cannot be empty.",
                nameof(replacementSessionId));
        }

        ReplacedBySessionId = replacementSessionId;
        RevokedAt ??= DateTimeOffset.UtcNow;
    }
}