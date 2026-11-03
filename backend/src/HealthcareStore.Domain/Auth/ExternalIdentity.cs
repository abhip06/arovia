using HealthcareStore.Domain.Common;

namespace HealthcareStore.Domain.Auth;

public sealed class ExternalIdentity : Entity
{
    private ExternalIdentity()
    {
        // Required by EF Core.
    }

    private ExternalIdentity(
        Guid userId,
        ExternalIdentityProvider provider,
        string providerSubject)
    {
        UserId = userId;
        Provider = provider;
        ProviderSubject = providerSubject;

        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public Guid UserId { get; private set; }

    public ExternalIdentityProvider Provider { get; private set; }

    public string ProviderSubject { get; private set; } = null!;

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public static ExternalIdentity Create(
        Guid userId,
        ExternalIdentityProvider provider,
        string providerSubject)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException(
                "User ID cannot be empty.",
                nameof(userId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(
            providerSubject,
            nameof(providerSubject));

        return new ExternalIdentity(
            userId,
            provider,
            providerSubject.Trim());
    }
}