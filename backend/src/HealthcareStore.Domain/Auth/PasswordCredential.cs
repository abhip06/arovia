using HealthcareStore.Domain.Common;

namespace HealthcareStore.Domain.Auth;

public sealed class PasswordCredential : Entity
{
    private PasswordCredential()
    {
        // Required by EF Core.
    }

    private PasswordCredential(Guid userId, string hashedPassword)
    {
        UserId = userId;
        HashedPassword = hashedPassword;

        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public Guid UserId { get; private set; }

    public string HashedPassword { get; private set; } = null!;

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public static PasswordCredential Create(
        Guid userId,
        string hashedPassword)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException(
                "User ID cannot be empty.",
                nameof(userId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(
            hashedPassword,
            nameof(hashedPassword));

        return new PasswordCredential(
            userId,
            hashedPassword);
    }

    public void ChangePassword(string newHashedPassword)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            newHashedPassword,
            nameof(newHashedPassword));

        HashedPassword = newHashedPassword;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}