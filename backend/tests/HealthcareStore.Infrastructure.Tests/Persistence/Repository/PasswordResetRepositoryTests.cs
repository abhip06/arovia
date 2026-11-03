using HealthcareStore.Domain.Auth;
using HealthcareStore.Domain.Users;
using HealthcareStore.Infrastructure.Persistence;
using HealthcareStore.Infrastructure.Persistence.Repositories;
using HealthcareStore.Infrastructure.Tests.Database;
using Microsoft.EntityFrameworkCore;

namespace HealthcareStore.Infrastructure.Tests.Persistence.Repositories;

public sealed class PasswordResetRepositoryTests
    : IClassFixture<PostgreSqlFixture>
{
    private readonly PostgreSqlFixture _fixture;

    public PasswordResetRepositoryTests(
        PostgreSqlFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task TryCompleteAsync_ShouldChangePasswordAndConsumeToken()
    {
        await using var dbContext =
            _fixture.CreateDbContext();

        await dbContext.Database.MigrateAsync();

        var email =
            $"john-{Guid.NewGuid():N}@example.com";

        var user = User.Create(
            "John",
            "Doe",
            email);

        var credential = PasswordCredential.Create(
            user.Id,
            "HASHED:OLD-PASSWORD");

        var token = PasswordResetToken.Create(
            user.Id,
            "HASHED:RESET-TOKEN",
            DateTimeOffset.UtcNow.AddHours(1));

        dbContext.Users.Add(user);
        dbContext.PasswordCredentials.Add(credential);
        dbContext.PasswordResetTokens.Add(token);

        await dbContext.SaveChangesAsync();

        var repository =
            new PasswordResetRepository(dbContext);

        var result =
            await repository.TryCompleteAsync(
                user,
                credential,
                token,
                "HASHED:NEW-PASSWORD");

        Assert.True(result);

        var storedToken =
            await dbContext.PasswordResetTokens
                .AsNoTracking()
                .SingleAsync(
                    x => x.Id == token.Id);

        var storedCredential =
            await dbContext.PasswordCredentials
                .AsNoTracking()
                .SingleAsync(
                    x => x.UserId == user.Id);

        Assert.NotNull(storedToken.UsedAt);

        Assert.Null(storedToken.RevokedAt);

        Assert.Equal(
            "HASHED:NEW-PASSWORD",
            storedCredential.HashedPassword);
    }
}