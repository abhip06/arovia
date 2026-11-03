using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Application.Abstractions.Security;
using HealthcareStore.Application.Common.Exceptions;
using HealthcareStore.Application.Auth.RefreshToken;
using HealthcareStore.Domain.Auth;
using HealthcareStore.Domain.Users;

namespace HealthcareStore.Application.Tests.Auth.RefreshToken;

public sealed class RefreshTokenServiceTests
{
    [Fact]
    public async Task RefreshAsync_ShouldRotateSessionAndReturnNewTokens_WhenRefreshTokenIsValid()
    {
        var user = User.Create(
            "John",
            "Doe",
            "john@example.com");

        var tokenFamilyId = Guid.NewGuid();

        var currentSession = UserSession.Create(
            user.Id,
            "HASHED:CURRENT-TOKEN",
            tokenFamilyId,
            DateTimeOffset.UtcNow.AddDays(30));

        var sessionRepository = new FakeUserSessionRepository
        {
            RefreshResult = (user, currentSession)
        };

        var accessTokenGenerator =
            new FakeAccessTokenGenerator();

        var refreshTokenGenerator =
            new FakeRefreshTokenGenerator();

        var service = new RefreshTokenService(
            sessionRepository,
            accessTokenGenerator,
            refreshTokenGenerator);

        var response = await service.RefreshAsync(
            new RefreshTokenRequest("CURRENT-TOKEN"));

        Assert.Equal(
            "ACCESS-TOKEN",
            response.AccessToken);

        Assert.Equal(
            "NEW-REFRESH-TOKEN",
            response.RefreshToken);

        Assert.Equal(
            900,
            response.ExpiresIn);

        Assert.Equal(
            "CURRENT-TOKEN",
            refreshTokenGenerator.HashedTokens[0]);

        Assert.Equal(
            "NEW-REFRESH-TOKEN",
            refreshTokenGenerator.HashedTokens[1]);

        Assert.Equal(
            1,
            refreshTokenGenerator.GenerateCallCount);

        Assert.Same(
            user,
            accessTokenGenerator.LastUser);

        Assert.Equal(
            sessionRepository.ReplacementSession!.Id,
            accessTokenGenerator.LastSessionId);

        Assert.Equal(
            1,
            accessTokenGenerator.GenerateCallCount);

        Assert.NotNull(
            sessionRepository.ReplacementSession);

        Assert.Equal(
            user.Id,
            sessionRepository.ReplacementSession!.UserId);

        Assert.Equal(
            tokenFamilyId,
            sessionRepository.ReplacementSession.TokenFamilyId);

        Assert.Equal(
            "HASHED:NEW-REFRESH-TOKEN",
            sessionRepository.ReplacementSession.RefreshTokenHash);

        Assert.Null(
            sessionRepository.ReplacementSession.RevokedAt);

        Assert.Equal(
            1,
            sessionRepository.RotateCallCount);
    }

    [Fact]
    public async Task RefreshAsync_ShouldRejectUnknownRefreshToken()
    {
        var sessionRepository = new FakeUserSessionRepository
        {
            RefreshResult = null
        };

        var service = new RefreshTokenService(
            sessionRepository,
            new FakeAccessTokenGenerator(),
            new FakeRefreshTokenGenerator());

        var exception =
            await Assert.ThrowsAsync<UnauthorizedException>(
                () => service.RefreshAsync(
                    new RefreshTokenRequest("INVALID-TOKEN")));

        Assert.Equal(
            "Invalid refresh token.",
            exception.Message);

        Assert.Equal(
            0,
            sessionRepository.RotateCallCount);
    }

    [Fact]
    public async Task RefreshAsync_ShouldRejectExpiredRefreshToken()
    {
        var user = User.Create(
            "John",
            "Doe",
            "john@example.com");

        var session = UserSession.Create(
            user.Id,
            "HASHED:CURRENT-TOKEN",
            Guid.NewGuid(),
            DateTimeOffset.UtcNow.AddMinutes(1));

        var sessionRepository = new FakeUserSessionRepository
        {
            RefreshResult = (user, session)
        };

        sessionRepository.ForceExpiration(session);

        var service = new RefreshTokenService(
            sessionRepository,
            new FakeAccessTokenGenerator(),
            new FakeRefreshTokenGenerator());

        var exception =
            await Assert.ThrowsAsync<UnauthorizedException>(
                () => service.RefreshAsync(
                    new RefreshTokenRequest("CURRENT-TOKEN")));

        Assert.Equal(
            "Invalid refresh token.",
            exception.Message);

        Assert.Equal(
            0,
            sessionRepository.RotateCallCount);
    }

    [Fact]
    public async Task RefreshAsync_ShouldRejectRevokedRefreshToken()
    {
        var user = User.Create(
            "John",
            "Doe",
            "john@example.com");

        var session = UserSession.Create(
            user.Id,
            "HASHED:CURRENT-TOKEN",
            Guid.NewGuid(),
            DateTimeOffset.UtcNow.AddDays(30));

        session.Revoke();

        var sessionRepository = new FakeUserSessionRepository
        {
            RefreshResult = (user, session)
        };

        var service = new RefreshTokenService(
            sessionRepository,
            new FakeAccessTokenGenerator(),
            new FakeRefreshTokenGenerator());

        var exception =
            await Assert.ThrowsAsync<UnauthorizedException>(
                () => service.RefreshAsync(
                    new RefreshTokenRequest("CURRENT-TOKEN")));

        Assert.Equal(
            "Invalid refresh token.",
            exception.Message);

        Assert.Equal(
            0,
            sessionRepository.RotateCallCount);

        Assert.Equal(
            0,
            sessionRepository.RevokeFamilyCallCount);
    }

    [Fact]
    public async Task RefreshAsync_ShouldRevokeTokenFamily_WhenRotatedTokenIsReused()
    {
        var user = User.Create(
            "John",
            "Doe",
            "john@example.com");

        var tokenFamilyId = Guid.NewGuid();

        var session = UserSession.Create(
            user.Id,
            "HASHED:CURRENT-TOKEN",
            tokenFamilyId,
            DateTimeOffset.UtcNow.AddDays(30));

        session.MarkReplacedBy(Guid.NewGuid());

        var sessionRepository = new FakeUserSessionRepository
        {
            RefreshResult = (user, session)
        };

        var service = new RefreshTokenService(
            sessionRepository,
            new FakeAccessTokenGenerator(),
            new FakeRefreshTokenGenerator());

        var exception =
            await Assert.ThrowsAsync<UnauthorizedException>(
                () => service.RefreshAsync(
                    new RefreshTokenRequest("CURRENT-TOKEN")));

        Assert.Equal(
            "Invalid refresh token.",
            exception.Message);

        Assert.Equal(
            tokenFamilyId,
            sessionRepository.RevokedFamilyId);

        Assert.Equal(
            1,
            sessionRepository.RevokeFamilyCallCount);

        Assert.Equal(
            0,
            sessionRepository.RotateCallCount);
    }

    [Fact]
    public async Task RefreshAsync_ShouldRejectMissingRefreshToken()
    {
        var service = new RefreshTokenService(
            new FakeUserSessionRepository(),
            new FakeAccessTokenGenerator(),
            new FakeRefreshTokenGenerator());

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.RefreshAsync(
                new RefreshTokenRequest("")));
    }

    [Fact]
    public async Task RefreshAsync_ShouldRejectWhenRotationFails()
    {
        var user = User.Create(
            "John",
            "Doe",
            "john@example.com");

        var session = UserSession.Create(
            user.Id,
            "HASHED:CURRENT-TOKEN",
            Guid.NewGuid(),
            DateTimeOffset.UtcNow.AddDays(30));

        var sessionRepository = new FakeUserSessionRepository
        {
            RefreshResult = (user, session),
            RotateResult = false
        };

        var service = new RefreshTokenService(
            sessionRepository,
            new FakeAccessTokenGenerator(),
            new FakeRefreshTokenGenerator());

        var exception =
            await Assert.ThrowsAsync<UnauthorizedException>(
                () => service.RefreshAsync(
                    new RefreshTokenRequest("CURRENT-TOKEN")));

        Assert.Equal(
            "Invalid refresh token.",
            exception.Message);

        Assert.Equal(
            1,
            sessionRepository.RotateCallCount);
    }

    private sealed class FakeUserSessionRepository
        : IUserSessionRepository
    {
        public (
            User User,
            UserSession Session)? RefreshResult
        {
            get;
            set;
        }

        public UserSession? ReplacementSession
        {
            get;
            private set;
        }

        public int RotateCallCount
        {
            get;
            private set;
        }

        public Guid? RevokedFamilyId
        {
            get;
            private set;
        }

        public int RevokeFamilyCallCount
        {
            get;
            private set;
        }

        public bool RotateResult { get; set; } = true;

        public Task AddAsync(
            UserSession session,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<(User User, UserSession Session)?>
            FindForRefreshAsync(
                string refreshTokenHash,
                CancellationToken cancellationToken = default)
        {
            return Task.FromResult(RefreshResult);
        }

        public Task<bool> TryRotateAsync(
            UserSession currentSession,
            UserSession replacementSession,
            CancellationToken cancellationToken = default)
        {
            ReplacementSession = replacementSession;
            RotateCallCount++;

            return Task.FromResult(RotateResult);
        }

        public Task RevokeFamilyAsync(
            Guid tokenFamilyId,
            CancellationToken cancellationToken = default)
        {
            RevokedFamilyId = tokenFamilyId;
            RevokeFamilyCallCount++;

            return Task.CompletedTask;
        }

        public Task<bool> RevokeAsync(
            string refreshTokenHash,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task RevokeAllAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<bool> IsActiveAsync(
            Guid sessionId,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public void ForceExpiration(UserSession session)
        {
            var property =
                typeof(UserSession).GetProperty(
                    nameof(UserSession.ExpiresAt));

            property!.SetValue(
                session,
                DateTimeOffset.UtcNow.AddMinutes(-1));
        }
    }

    private sealed class FakeAccessTokenGenerator
        : IAccessTokenGenerator
    {
        public User? LastUser
        {
            get;
            private set;
        }

        public Guid LastSessionId
        {
            get;
            private set;
        }

        public int GenerateCallCount
        {
            get;
            private set;
        }

        public AccessTokenResult Generate(
            User user,
            Guid sessionId)
        {
            LastUser = user;
            LastSessionId = sessionId;
            GenerateCallCount++;

            return new AccessTokenResult(
                "ACCESS-TOKEN",
                900);
        }
    }

    private sealed class FakeRefreshTokenGenerator
        : IRefreshTokenGenerator
    {
        public int GenerateCallCount
        {
            get;
            private set;
        }

        public List<string> HashedTokens { get; } = [];

        public string Generate()
        {
            GenerateCallCount++;

            return "NEW-REFRESH-TOKEN";
        }

        public string Hash(string token)
        {
            HashedTokens.Add(token);

            return $"HASHED:{token}";
        }
    }
}