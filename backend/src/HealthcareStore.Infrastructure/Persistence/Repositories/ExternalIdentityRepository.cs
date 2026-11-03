using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Domain.Auth;
using HealthcareStore.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace HealthcareStore.Infrastructure.Persistence.Repositories;

public sealed class ExternalIdentityRepository(
    ApplicationDbContext dbContext)
    : IExternalIdentityRepository
{
    public async Task<(User User, ExternalIdentity Identity)?> FindAsync(
        ExternalIdentityProvider provider,
        string providerSubject,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(providerSubject);

        var result = await dbContext.ExternalIdentities
            .Join(
                dbContext.Users,
                identity => identity.UserId,
                user => user.Id,
                (identity, user) => new
                {
                    Identity = identity,
                    User = user
                })
            .SingleOrDefaultAsync(
                x =>
                    x.Identity.Provider == provider &&
                    x.Identity.ProviderSubject == providerSubject,
                cancellationToken);

        return result is null
            ? null
            : (result.User, result.Identity);
    }

    public async Task AddAsync(
        ExternalIdentity identity,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(identity);

        dbContext.ExternalIdentities.Add(identity);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<ExternalIdentity?> FindByUserAsync(
        Guid userId,
        ExternalIdentityProvider provider,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.ExternalIdentities
            .SingleOrDefaultAsync(
                x =>
                    x.UserId == userId &&
                    x.Provider == provider,
                cancellationToken);
    }

    public async Task<bool> TryAddAsync(
        ExternalIdentity identity,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(identity);

        dbContext.ExternalIdentities.Add(identity);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is PostgresException
            {
                SqlState: PostgresErrorCodes.UniqueViolation
            })
        {
            dbContext.ChangeTracker.Clear();
            return false;
        }
    }
}