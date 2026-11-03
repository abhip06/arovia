using HealthcareStore.Domain.Auth;
using HealthcareStore.Domain.Users;
using HealthcareStore.Infrastructure.Persistence;
using HealthcareStore.Infrastructure.Persistence.Repositories;
using HealthcareStore.Infrastructure.Tests.Database;
using Microsoft.EntityFrameworkCore;

namespace HealthcareStore.Infrastructure.Tests.Persistence.Repositories;

public sealed class UserSessionRepositoryTests
: IClassFixture<PostgreSqlFixture>
{
    private readonly PostgreSqlFixture _fixture;

    public UserSessionRepositoryTests(
    PostgreSqlFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task TryRotateAsync_ShouldRevokeCurrentSessionAndCreateReplacement()
    {
        await using var dbContext = _fixture.CreateDbContext();

        await dbContext.Database.MigrateAsync();

        var email = $"john-{Guid.NewGuid():N}@example.com";

        var user = User.Create(
            "John",
            "Doe",
            email);

        var tokenFamilyId = Guid.NewGuid();

        var currentRefreshTokenHash =
            $"HASHED:CURRENT-{Guid.NewGuid():N}";

        var newRefreshTokenHash =
            $"HASHED:NEW-{Guid.NewGuid():N}";

        var currentSession = UserSession.Create(
            user.Id,
            currentRefreshTokenHash,
            tokenFamilyId,
            DateTimeOffset.UtcNow.AddDays(30));

        var replacementSession = UserSession.Create(
            user.Id,
            newRefreshTokenHash,
            tokenFamilyId,
            DateTimeOffset.UtcNow.AddDays(30));

        dbContext.Users.Add(user);
        dbContext.UserSessions.Add(currentSession);

        await dbContext.SaveChangesAsync();

        var repository = new UserSessionRepository(dbContext);

        var result = await repository.TryRotateAsync(
            currentSession,
            replacementSession);

        Assert.True(result);

        var storedCurrentSession =
            await dbContext.UserSessions
                .AsNoTracking()
                .SingleAsync(
                    session => session.Id == currentSession.Id);

        var storedReplacementSession =
            await dbContext.UserSessions
                .AsNoTracking()
                .SingleAsync(
                    session => session.Id == replacementSession.Id);

        Assert.NotNull(storedCurrentSession.RevokedAt);
        Assert.NotNull(storedCurrentSession.LastUsedAt);

        Assert.Equal(
            replacementSession.Id,
            storedCurrentSession.ReplacedBySessionId);

        Assert.Equal(
            tokenFamilyId,
            storedReplacementSession.TokenFamilyId);

        Assert.Equal(
            newRefreshTokenHash,
            storedReplacementSession.RefreshTokenHash);

        Assert.Null(storedReplacementSession.RevokedAt);
    }

    [Fact]
    public async Task TryRotateAsync_ShouldAllowOnlyOneConcurrentRotation()
    {
        await using var setupContext = _fixture.CreateDbContext();

        await setupContext.Database.MigrateAsync();

        var email = $"john-{Guid.NewGuid():N}@example.com";

        var user = User.Create(
            "John",
            "Doe",
            email);

        var tokenFamilyId = Guid.NewGuid();

        var currentRefreshTokenHash =
            $"HASHED:CURRENT-{Guid.NewGuid():N}";

        var currentSession = UserSession.Create(
            user.Id,
            currentRefreshTokenHash,
            tokenFamilyId,
            DateTimeOffset.UtcNow.AddDays(30));

        setupContext.Users.Add(user);
        setupContext.UserSessions.Add(currentSession);

        await setupContext.SaveChangesAsync();

        var firstReplacement = UserSession.Create(
            user.Id,
            $"HASHED:REFRESH-ONE-{Guid.NewGuid():N}",
            tokenFamilyId,
            DateTimeOffset.UtcNow.AddDays(30));

        var secondReplacement = UserSession.Create(
            user.Id,
            $"HASHED:REFRESH-TWO-{Guid.NewGuid():N}",
            tokenFamilyId,
            DateTimeOffset.UtcNow.AddDays(30));

        await using var firstContext =
            _fixture.CreateDbContext();

        await using var secondContext =
            _fixture.CreateDbContext();

        var firstSession =
            await firstContext.UserSessions
                .SingleAsync(
                    session => session.Id == currentSession.Id);

        var secondSession =
            await secondContext.UserSessions
                .SingleAsync(
                    session => session.Id == currentSession.Id);

        var firstRepository =
            new UserSessionRepository(firstContext);

        var secondRepository =
            new UserSessionRepository(secondContext);

        var firstTask = firstRepository.TryRotateAsync(
            firstSession,
            firstReplacement);

        var secondTask = secondRepository.TryRotateAsync(
            secondSession,
            secondReplacement);

        var results = await Task.WhenAll(
            firstTask,
            secondTask);

        Assert.Equal(
            1,
            results.Count(result => result));

        Assert.Equal(
            1,
            results.Count(result => !result));

        await using var verificationContext =
            _fixture.CreateDbContext();

        var sessions = await verificationContext.UserSessions
            .AsNoTracking()
            .Where(session =>
                session.TokenFamilyId == tokenFamilyId)
            .ToListAsync();

        Assert.Equal(
            2,
            sessions.Count);

        Assert.Equal(
            1,
            sessions.Count(session => session.RevokedAt == null));

        Assert.Equal(
            1,
            sessions.Count(session => session.RevokedAt != null));
    }
}
