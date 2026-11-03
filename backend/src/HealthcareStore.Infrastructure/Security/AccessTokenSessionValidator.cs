using HealthcareStore.Application.Abstractions.Security;
using HealthcareStore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HealthcareStore.Infrastructure.Security;

internal class AccessTokenSessionValidator : IAccessTokenSessionValidator
{
    private readonly ApplicationDbContext _dbContext;

    public AccessTokenSessionValidator(
        ApplicationDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        _dbContext = dbContext;
    }

    public async Task<bool> IsActiveAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        if (sessionId == Guid.Empty)
        {
            return false;
        }

        var now = DateTimeOffset.UtcNow;

        return await _dbContext.UserSessions
            .AnyAsync(
                session =>
                    session.Id == sessionId &&
                    session.RevokedAt == null &&
                    session.ExpiresAt > now,
                cancellationToken);
    }
}
