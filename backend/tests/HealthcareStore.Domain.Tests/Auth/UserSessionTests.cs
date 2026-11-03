using HealthcareStore.Domain.Auth;

namespace HealthcareStore.Domain.Tests.Auth;

public sealed class UserSessionTests
{
    [Fact]
    public void Create_ShouldCreateSession_WithValidData()
    {
        var userId = Guid.NewGuid();
        var tokenFamilyId = Guid.NewGuid();
        const string refreshTokenHash = "hashed-refresh-token";
        var expiresAt = DateTimeOffset.UtcNow.AddDays(30);

        var session = UserSession.Create(
            userId,
            refreshTokenHash,
            tokenFamilyId,
            expiresAt);

        Assert.NotEqual(Guid.Empty, session.Id);
        Assert.Equal(userId, session.UserId);
        Assert.Equal(refreshTokenHash, session.RefreshTokenHash);
        Assert.Equal(tokenFamilyId, session.TokenFamilyId);
        Assert.Equal(expiresAt, session.ExpiresAt);
        Assert.Null(session.LastUsedAt);
        Assert.Null(session.RevokedAt);
        Assert.Null(session.ReplacedBySessionId);
        Assert.NotEqual(default, session.CreatedAt);
    }

    [Fact]
    public void Create_ShouldThrow_WhenUserIdIsEmpty()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            UserSession.Create(
                Guid.Empty,
                "hashed-refresh-token",
                Guid.NewGuid(),
                DateTimeOffset.UtcNow.AddDays(30)));

        Assert.Equal("userId", exception.ParamName);
    }

    [Fact]
    public void Create_ShouldThrow_WhenRefreshTokenHashIsEmpty()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            UserSession.Create(
                Guid.NewGuid(),
                "",
                Guid.NewGuid(),
                DateTimeOffset.UtcNow.AddDays(30)));

        Assert.Equal("refreshTokenHash", exception.ParamName);
    }

    [Fact]
    public void Create_ShouldThrow_WhenTokenFamilyIdIsEmpty()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            UserSession.Create(
                Guid.NewGuid(),
                "hashed-refresh-token",
                Guid.Empty,
                DateTimeOffset.UtcNow.AddDays(30)));

        Assert.Equal("tokenFamilyId", exception.ParamName);
    }

    [Fact]
    public void Create_ShouldThrow_WhenExpirationIsNotInFuture()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            UserSession.Create(
                Guid.NewGuid(),
                "hashed-refresh-token",
                Guid.NewGuid(),
                DateTimeOffset.UtcNow));

        Assert.Equal("expiresAt", exception.ParamName);
    }

    [Fact]
    public void MarkUsed_ShouldSetLastUsedAt()
    {
        var session = UserSession.Create(
            Guid.NewGuid(),
            "hashed-refresh-token",
            Guid.NewGuid(),
            DateTimeOffset.UtcNow.AddDays(30));

        Assert.Null(session.LastUsedAt);

        session.MarkUsed();

        Assert.NotNull(session.LastUsedAt);
    }

    [Fact]
    public void Revoke_ShouldSetRevokedAt()
    {
        var session = UserSession.Create(
            Guid.NewGuid(),
            "hashed-refresh-token",
            Guid.NewGuid(),
            DateTimeOffset.UtcNow.AddDays(30));

        Assert.Null(session.RevokedAt);

        session.Revoke();

        Assert.NotNull(session.RevokedAt);
    }

    [Fact]
    public void Revoke_ShouldBeIdempotent()
    {
        var session = UserSession.Create(
            Guid.NewGuid(),
            "hashed-refresh-token",
            Guid.NewGuid(),
            DateTimeOffset.UtcNow.AddDays(30));

        session.Revoke();
        var revokedAt = session.RevokedAt;

        session.Revoke();

        Assert.Equal(revokedAt, session.RevokedAt);
    }

    [Fact]
    public void MarkReplacedBy_ShouldSetReplacementAndRevokeSession()
    {
        var session = UserSession.Create(
            Guid.NewGuid(),
            "hashed-refresh-token",
            Guid.NewGuid(),
            DateTimeOffset.UtcNow.AddDays(30));

        var replacementSessionId = Guid.NewGuid();

        session.MarkReplacedBy(replacementSessionId);

        Assert.Equal(
            replacementSessionId,
            session.ReplacedBySessionId);

        Assert.NotNull(session.RevokedAt);
    }

    [Fact]
    public void MarkReplacedBy_ShouldThrow_WhenReplacementIdIsEmpty()
    {
        var session = UserSession.Create(
            Guid.NewGuid(),
            "hashed-refresh-token",
            Guid.NewGuid(),
            DateTimeOffset.UtcNow.AddDays(30));

        var exception = Assert.Throws<ArgumentException>(() =>
            session.MarkReplacedBy(Guid.Empty));

        Assert.Equal(
            "replacementSessionId",
            exception.ParamName);
    }
}
