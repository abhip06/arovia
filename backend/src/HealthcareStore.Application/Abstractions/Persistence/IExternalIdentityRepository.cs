using HealthcareStore.Domain.Auth;
using HealthcareStore.Domain.Users;

namespace HealthcareStore.Application.Abstractions.Persistence;

public interface IExternalIdentityRepository
{
    Task<(User User, ExternalIdentity Identity)?> FindAsync(
        ExternalIdentityProvider provider,
        string providerSubject,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        ExternalIdentity identity,
        CancellationToken cancellationToken = default);

    Task<ExternalIdentity?> FindByUserAsync(
        Guid userId,
        ExternalIdentityProvider provider,
        CancellationToken cancellationToken = default);

    Task<bool> TryAddAsync(
        ExternalIdentity identity,
        CancellationToken cancellationToken = default);
}