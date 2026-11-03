using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Domain.Auth;
using HealthcareStore.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace HealthcareStore.Infrastructure.Persistence.Repositories;

public sealed class EmailVerificationRepository : IEmailVerificationRepository
{
    private readonly ApplicationDbContext _dbContext;

    public EmailVerificationRepository(ApplicationDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        _dbContext = dbContext;
    }

    public async Task<(User User, EmailVerificationToken Token)?> FindAsync(
        string tokenHash,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tokenHash, nameof(tokenHash));

        var result = await _dbContext.EmailVerificationTokens
            .Where(token => token.TokenHash == tokenHash)
            .Join(
                _dbContext.Users,
                token => token.UserId,
                user => user.Id,
                (token, user) => new
                {
                    User = user,
                    Token = token
                })
            .SingleOrDefaultAsync(cancellationToken);

        return result is null
            ? null
            : (result.User, result.Token);
    }

    public async Task<bool> TryCompleteAsync(
    User user,
    EmailVerificationToken token,
    CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentNullException.ThrowIfNull(token);

        var now = DateTimeOffset.UtcNow;

        await using var transaction =
            await _dbContext.Database.BeginTransactionAsync(
                cancellationToken);

        var affectedRows = await _dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"""
            UPDATE email_verification_tokens
            SET "UsedAt" = {now}
            WHERE "Id" = {token.Id}
              AND "UsedAt" IS NULL
              AND "RevokedAt" IS NULL
              AND "ExpiresAt" > {now}
            """,
            cancellationToken);

        if (affectedRows != 1)
        {
            await transaction.RollbackAsync(cancellationToken);

            return false;
        }

        await _dbContext.Entry(token).ReloadAsync(cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return true;
    }

    public async Task<bool> ReplaceAsync(
        Guid userId,
        EmailVerificationToken newToken,
        CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException(
                "User ID cannot be empty.",
                nameof(userId));
        }

        ArgumentNullException.ThrowIfNull(newToken);

        var activeTokens = await _dbContext.EmailVerificationTokens
            .Where(token =>
                token.UserId == userId &&
                !token.UsedAt.HasValue &&
                !token.RevokedAt.HasValue)
            .ToListAsync(cancellationToken);

        foreach (var token in activeTokens)
        {
            token.Revoke();
        }

        await _dbContext.EmailVerificationTokens.AddAsync(
            newToken,
            cancellationToken);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);

            return true;
        }
        catch (PostgresException exception)
            when (exception.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            _dbContext.ChangeTracker.Clear();

            return false;
        }
    }
}