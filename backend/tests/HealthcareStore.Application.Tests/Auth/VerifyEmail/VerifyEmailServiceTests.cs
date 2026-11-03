using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Application.Abstractions.Security;
using HealthcareStore.Application.Common.Exceptions;
using HealthcareStore.Application.Auth.VerifyEmail;
using HealthcareStore.Domain.Auth;
using HealthcareStore.Domain.Users;

namespace HealthcareStore.Application.Tests.Auth.VerifyEmail;

public sealed class VerifyEmailServiceTests
{
    [Fact]
    public async Task VerifyAsync_ShouldVerifyUserAndMarkTokenAsUsed_WhenTokenIsValid()
    {
        var user = User.Create(
            "John",
            "Doe",
            "john@example.com");

        var token = EmailVerificationToken.Create(
            user.Id,
            "HASHED:VERIFICATION-TOKEN",
            DateTimeOffset.UtcNow.AddHours(1));

        var repository = new FakeEmailVerificationRepository
        {
            Result = (user, token)
        };

        var tokenGenerator = new FakeVerificationTokenGenerator();

        var service = new VerifyEmailService(
            repository,
            tokenGenerator);

        var response = await service.VerifyAsync(
            new VerifyEmailRequest("VERIFICATION-TOKEN"));

        Assert.Equal(user.Id, response.UserId);
        Assert.Equal("john@example.com", response.Email);
        Assert.True(response.IsEmailVerified);

        Assert.True(user.IsEmailVerified);
        Assert.NotNull(token.UsedAt);

        Assert.Equal(1, repository.CompleteCallCount);
        Assert.Same(user, repository.CompletedUser);
        Assert.Same(token, repository.CompletedToken);
    }

    [Fact]
    public async Task VerifyAsync_ShouldRejectInvalidToken()
    {
        var repository = new FakeEmailVerificationRepository
        {
            Result = null
        };

        var service = new VerifyEmailService(
            repository,
            new FakeVerificationTokenGenerator());

        var exception =
            await Assert.ThrowsAsync<InvalidVerificationTokenException>(
                () => service.VerifyAsync(
                    new VerifyEmailRequest("INVALID-TOKEN")));

        Assert.Equal(
            "The email verification token is invalid.",
            exception.Message);

        Assert.Equal(0, repository.CompleteCallCount);
    }

    [Fact]
    public async Task VerifyAsync_ShouldRejectAlreadyUsedToken()
    {
        var user = User.Create(
            "John",
            "Doe",
            "john@example.com");

        var token = EmailVerificationToken.Create(
            user.Id,
            "HASHED:VERIFICATION-TOKEN",
            DateTimeOffset.UtcNow.AddHours(1));

        token.MarkUsed();

        var repository = new FakeEmailVerificationRepository
        {
            Result = (user, token)
        };

        var service = new VerifyEmailService(
            repository,
            new FakeVerificationTokenGenerator());

        var exception =
            await Assert.ThrowsAsync<InvalidVerificationTokenException>(
                () => service.VerifyAsync(
                    new VerifyEmailRequest("VERIFICATION-TOKEN")));

        Assert.Equal(
            "The email verification token has already been used.",
            exception.Message);

        Assert.Equal(0, repository.CompleteCallCount);
    }

    [Fact]
    public async Task VerifyAsync_ShouldRejectRevokedToken()
    {
        var user = User.Create(
            "John",
            "Doe",
            "john@example.com");

        var token = EmailVerificationToken.Create(
            user.Id,
            "HASHED:VERIFICATION-TOKEN",
            DateTimeOffset.UtcNow.AddHours(1));

        token.Revoke();

        var repository = new FakeEmailVerificationRepository
        {
            Result = (user, token)
        };

        var service = new VerifyEmailService(
            repository,
            new FakeVerificationTokenGenerator());

        var exception =
            await Assert.ThrowsAsync<InvalidVerificationTokenException>(
                () => service.VerifyAsync(
                    new VerifyEmailRequest("VERIFICATION-TOKEN")));

        Assert.Equal(
            "The email verification token has been revoked.",
            exception.Message);

        Assert.Equal(0, repository.CompleteCallCount);
    }

    [Fact]
    public async Task VerifyAsync_ShouldReturnSuccess_WhenUserIsAlreadyVerified()
    {
        var user = User.Create(
            "John",
            "Doe",
            "john@example.com");

        user.VerifyEmail();

        var token = EmailVerificationToken.Create(
            user.Id,
            "HASHED:VERIFICATION-TOKEN",
            DateTimeOffset.UtcNow.AddHours(1));

        var repository = new FakeEmailVerificationRepository
        {
            Result = (user, token)
        };

        var service = new VerifyEmailService(
            repository,
            new FakeVerificationTokenGenerator());

        var response = await service.VerifyAsync(
            new VerifyEmailRequest("VERIFICATION-TOKEN"));

        Assert.Equal(user.Id, response.UserId);
        Assert.Equal("john@example.com", response.Email);
        Assert.True(response.IsEmailVerified);

        Assert.Equal(0, repository.CompleteCallCount);
        Assert.Null(token.UsedAt);
    }

    [Fact]
    public async Task VerifyAsync_ShouldRejectMissingToken()
    {
        var service = new VerifyEmailService(
            new FakeEmailVerificationRepository(),
            new FakeVerificationTokenGenerator());

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.VerifyAsync(
                new VerifyEmailRequest("")));
    }

    [Fact]
    public async Task VerifyAsync_ShouldRejectWhenTokenCouldNotBeConsumed()
    {
        var user = User.Create(
            "John",
            "Doe",
            "john@example.com");

        var token = EmailVerificationToken.Create(
            user.Id,
            "HASHED:VERIFICATION-TOKEN",
            DateTimeOffset.UtcNow.AddHours(1));

        var repository = new FakeEmailVerificationRepository
        {
            Result = (user, token),
            CompleteResult = false
        };

        var service = new VerifyEmailService(
            repository,
            new FakeVerificationTokenGenerator());

        var exception =
            await Assert.ThrowsAsync<InvalidVerificationTokenException>(
                () => service.VerifyAsync(
                    new VerifyEmailRequest("VERIFICATION-TOKEN")));

        Assert.Equal(
            "The email verification token is no longer valid.",
            exception.Message);

        Assert.Equal(1, repository.CompleteCallCount);
    }

    private sealed class FakeEmailVerificationRepository
        : IEmailVerificationRepository
    {
        public (User User, EmailVerificationToken Token)? Result { get; set; }

        public User? CompletedUser { get; private set; }

        public EmailVerificationToken? CompletedToken { get; private set; }

        public int CompleteCallCount { get; private set; }

        public bool CompleteResult { get; set; } = true;

        public Task<(User User, EmailVerificationToken Token)?> FindAsync(
            string tokenHash,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Result);
        }

        public Task<bool> TryCompleteAsync(
            User user,
            EmailVerificationToken token,
            CancellationToken cancellationToken = default)
        {
            CompletedUser = user;
            CompletedToken = token;
            CompleteCallCount++;

            return Task.FromResult(CompleteResult);
        }

        public Task<bool> ReplaceAsync(
            Guid userId,
            EmailVerificationToken newToken,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }
    }

    private sealed class FakeVerificationTokenGenerator
        : ITokenGenerator
    {
        public string Hash(string token)
        {
            return $"HASHED:{token}";
        }

        public string Generate()
        {
            return "VERIFICATION-TOKEN";
        }
    }
}