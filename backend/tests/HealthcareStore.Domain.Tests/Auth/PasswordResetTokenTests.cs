using HealthcareStore.Domain.Auth;

namespace HealthcareStore.Domain.Tests.Auth;

public sealed class PasswordResetTokenTests
{
    [Fact]
    public void Create_ShouldCreateToken_WithValidData()
    {
        var userId = Guid.NewGuid();
        const string tokenHash = "hashed-token";
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(30);

        var token = PasswordResetToken.Create(
            userId,
            tokenHash,
            expiresAt);

        Assert.NotEqual(Guid.Empty, token.Id);
        Assert.Equal(userId, token.UserId);
        Assert.Equal(tokenHash, token.TokenHash);
        Assert.Equal(expiresAt, token.ExpiresAt);
        Assert.Null(token.UsedAt);
        Assert.NotEqual(default, token.CreatedAt);
    }

    [Fact]
    public void Create_ShouldThrow_WhenUserIdIsEmpty()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            PasswordResetToken.Create(
                Guid.Empty,
                "hashed-token",
                DateTimeOffset.UtcNow.AddMinutes(30)));

        Assert.Equal("userId", exception.ParamName);
    }

    [Fact]
    public void Create_ShouldThrow_WhenTokenHashIsEmpty()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            PasswordResetToken.Create(
                Guid.NewGuid(),
                "",
                DateTimeOffset.UtcNow.AddMinutes(30)));

        Assert.Equal("tokenHash", exception.ParamName);
    }

    [Fact]
    public void Create_ShouldThrow_WhenExpirationIsNotInFuture()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            PasswordResetToken.Create(
                Guid.NewGuid(),
                "hashed-token",
                DateTimeOffset.UtcNow));

        Assert.Equal("expiresAt", exception.ParamName);
    }

    [Fact]
    public void MarkUsed_ShouldSetUsedAt()
    {
        var token = PasswordResetToken.Create(
            Guid.NewGuid(),
            "hashed-token",
            DateTimeOffset.UtcNow.AddMinutes(30));

        Assert.Null(token.UsedAt);

        token.MarkUsed();

        Assert.NotNull(token.UsedAt);
    }

    [Fact]
    public void MarkUsed_ShouldBeIdempotent()
    {
        var token = PasswordResetToken.Create(
            Guid.NewGuid(),
            "hashed-token",
            DateTimeOffset.UtcNow.AddMinutes(30));

        token.MarkUsed();
        var usedAt = token.UsedAt;

        token.MarkUsed();

        Assert.Equal(usedAt, token.UsedAt);
    }
}
