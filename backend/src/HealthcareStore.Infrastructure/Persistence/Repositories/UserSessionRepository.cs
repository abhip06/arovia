using HealthcareStore.Application.Abstractions.Security;
using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Domain.Auth;
using HealthcareStore.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace HealthcareStore.Infrastructure.Persistence.Repositories;

public sealed class UserSessionRepository
    : IUserSessionRepository
{
    private readonly ApplicationDbContext _dbContext;

    public UserSessionRepository(
        ApplicationDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        _dbContext = dbContext;
    }

    public async Task AddAsync(
        UserSession session,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(session);

        await _dbContext.UserSessions.AddAsync(
            session,
            cancellationToken);

        await _dbContext.SaveChangesAsync(
            cancellationToken);
    }

    public async Task<(User User, UserSession Session)?> FindForRefreshAsync(
        string refreshTokenHash,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            refreshTokenHash,
            nameof(refreshTokenHash));

        var result = await _dbContext.UserSessions
            .Join(
                _dbContext.Users,
                session => session.UserId,
                user => user.Id,
                (session, user) => new
                {
                    Session = session,
                    User = user
                })
            .SingleOrDefaultAsync(
                x => x.Session.RefreshTokenHash == refreshTokenHash,
                cancellationToken);

        return result is null
            ? null
            : (result.User, result.Session);
    }

    public async Task<bool> TryRotateAsync(
    UserSession currentSession,
    UserSession replacementSession,
    CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(currentSession);
        ArgumentNullException.ThrowIfNull(replacementSession);

        var now = DateTimeOffset.UtcNow;

        await using var transaction =
            await _dbContext.Database.BeginTransactionAsync(
                cancellationToken);

        await _dbContext.UserSessions.AddAsync(
            replacementSession,
            cancellationToken);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        var affectedRows =
            await _dbContext.Database.ExecuteSqlInterpolatedAsync(
                $"""
                UPDATE user_sessions
                SET
                    "LastUsedAt" = {now},
                    "RevokedAt" = {now},
                    "ReplacedBySessionId" = {replacementSession.Id}
                WHERE "Id" = {currentSession.Id}
                  AND "RevokedAt" IS NULL
                  AND "ReplacedBySessionId" IS NULL
                  AND "ExpiresAt" > {now}
                """,
                cancellationToken);

        if (affectedRows != 1)
        {
            await transaction.RollbackAsync(cancellationToken);

            _dbContext.ChangeTracker.Clear();

            return false;
        }

        await transaction.CommitAsync(
            cancellationToken);

        return true;
    }

    public async Task RevokeFamilyAsync(
        Guid tokenFamilyId,
        CancellationToken cancellationToken = default)
    {
        if (tokenFamilyId == Guid.Empty)
        {
            throw new ArgumentException(
                "Token family ID cannot be empty.",
                nameof(tokenFamilyId));
        }

        var now = DateTimeOffset.UtcNow;

        await _dbContext.UserSessions
            .Where(session =>
                session.TokenFamilyId == tokenFamilyId &&
                session.RevokedAt == null)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(
                        session => session.RevokedAt,
                        now),
                cancellationToken);
    }

    public async Task<bool> RevokeAsync(
        string refreshTokenHash,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(refreshTokenHash);

        var now = DateTimeOffset.UtcNow;

        var affectedRows = await _dbContext.UserSessions
            .Where(x =>
                x.RefreshTokenHash == refreshTokenHash &&
                x.RevokedAt == null)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(
                        x => x.RevokedAt,
                        now),
                cancellationToken);

        return affectedRows == 1;
    }

    public async Task RevokeAllAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException(
                "User ID cannot be empty.",
                nameof(userId));
        }

        var now = DateTimeOffset.UtcNow;

        await _dbContext.UserSessions
            .Where(x =>
                x.UserId == userId &&
                x.RevokedAt == null)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(
                        x => x.RevokedAt,
                        now),
                cancellationToken);
    }
}