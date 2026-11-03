using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Application.Abstractions.Security;
using HealthcareStore.Application.Auth.Password;
using HealthcareStore.Application.Common.Exceptions;
using HealthcareStore.Application.Users.GetAll;
using HealthcareStore.Domain.Auth;
using HealthcareStore.Domain.Users;

namespace HealthcareStore.Application.Tests.Auth.Password;

public sealed class ChangePasswordServiceTests
{
    [Fact]
    public async Task ChangeAsync_ShouldChangePassword_WhenCurrentPasswordIsValid()
    {
        var user = User.Create(
            "John",
            "Doe",
            "john@example.com");

        var credential = PasswordCredential.Create(
            user.Id,
            "HASHED:OldPassword123!");

        var repository = new FakeUserRepository
        {
            PasswordCredential = credential
        };

        var passwordHasher = new FakePasswordHasher();

        var sessionRepository = new FakeUserSessionRepository();

        var service = new ChangePasswordService(
            repository,
            passwordHasher,
            sessionRepository);

        await service.ChangeAsync(
            new ChangePasswordRequest(
                user.Id,
                "OldPassword123!",
                "NewPassword123!"));

        Assert.Equal(
            "HASHED:NewPassword123!",
            credential.HashedPassword);

        Assert.Equal(
            1,
            repository.SaveChangesCallCount);

        Assert.Equal(
            user.Id,
            sessionRepository.RevokedUserId);

        Assert.Equal(
            1,
            sessionRepository.RevokeAllCallCount);
    }

    [Fact]
    public async Task ChangeAsync_ShouldRejectInvalidCurrentPassword()
    {
        var user = User.Create(
            "John",
            "Doe",
            "john@example.com");

        var credential = PasswordCredential.Create(
            user.Id,
            "HASHED:OldPassword123!");

        var repository = new FakeUserRepository
        {
            PasswordCredential = credential
        };

        var passwordHasher = new FakePasswordHasher();

        var sessionRepository = new FakeUserSessionRepository();

        var service = new ChangePasswordService(
            repository,
            passwordHasher,
            sessionRepository);

        var exception =
            await Assert.ThrowsAsync<UnauthorizedException>(
                () => service.ChangeAsync(
                    new ChangePasswordRequest(
                        user.Id,
                        "WrongPassword123!",
                        "NewPassword123!")));

        Assert.Equal(
            "Current password is incorrect.",
            exception.Message);

        Assert.Equal(
            "HASHED:OldPassword123!",
            credential.HashedPassword);

        Assert.Equal(
            0,
            repository.SaveChangesCallCount);

        Assert.Equal(
            0,
            sessionRepository.RevokeAllCallCount);
    }

    [Fact]
    public async Task ChangeAsync_ShouldRejectSamePassword()
    {
        var user = User.Create(
            "John",
            "Doe",
            "john@example.com");

        var credential = PasswordCredential.Create(
            user.Id,
            "HASHED:OldPassword123!");

        var repository = new FakeUserRepository
        {
            PasswordCredential = credential
        };

        var service = new ChangePasswordService(
            repository,
            new FakePasswordHasher(),
            new FakeUserSessionRepository());

        var exception =
            await Assert.ThrowsAsync<ConflictException>(
                () => service.ChangeAsync(
                    new ChangePasswordRequest(
                        user.Id,
                        "OldPassword123!",
                        "OldPassword123!")));

        Assert.Equal(
            "New password must be different from the current password.",
            exception.Message);
    }

    [Fact]
    public async Task ChangeAsync_ShouldRejectMissingCredential()
    {
        var userId = Guid.NewGuid();

        var service = new ChangePasswordService(
            new FakeUserRepository(),
            new FakePasswordHasher(),
            new FakeUserSessionRepository());

        var exception =
            await Assert.ThrowsAsync<NotFoundException>(
                () => service.ChangeAsync(
                    new ChangePasswordRequest(
                        userId,
                        "OldPassword123!",
                        "NewPassword123!")));

        Assert.Equal(
            "Password credentials were not found.",
            exception.Message);
    }

    [Fact]
    public async Task ChangeAsync_ShouldRejectEmptyUserId()
    {
        var service = new ChangePasswordService(
            new FakeUserRepository(),
            new FakePasswordHasher(),
            new FakeUserSessionRepository());

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.ChangeAsync(
                new ChangePasswordRequest(
                    Guid.Empty,
                    "OldPassword123!",
                    "NewPassword123!")));
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        public PasswordCredential? PasswordCredential { get; set; }

        public int SaveChangesCallCount { get; private set; }

        public Task<bool> ExistsByEmailAsync(
            string email,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<bool> ExistsByPhoneAsync(
            string phone,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<User?> FindByEmailAsync(
            string email,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<(User User, PasswordCredential Credential)?>
            FindForAuthenticationAsync(
                string email,
                CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<User?> FindByIdAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<GetAllUsersDto> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<string?> GetPhoneAsync(Guid userId, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public Task AddAsync(
            User user,
            PasswordCredential credential,
            EmailVerificationToken emailVerificationToken,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task AddExternalUserAsync(
            User user,
            ExternalIdentity externalIdentity,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            SaveChangesCallCount++;
            return Task.CompletedTask;
        }

        public Task<PasswordCredential?>
            FindPasswordCredentialByUserIdAsync(
                Guid userId,
                CancellationToken cancellationToken = default)
        {
            return Task.FromResult(PasswordCredential);
        }
    }

    private sealed class FakePasswordHasher : IPasswordHasher
    {
        public string Hash(string password)
        {
            return $"HASHED:{password}";
        }

        public bool Verify(
            string password,
            string passwordHash)
        {
            return passwordHash == $"HASHED:{password}";
        }
    }

    private sealed class FakeUserSessionRepository
        : IUserSessionRepository
    {
        public Guid? RevokedUserId { get; private set; }

        public int RevokeAllCallCount { get; private set; }

        public Task RevokeAllAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            RevokedUserId = userId;
            RevokeAllCallCount++;

            return Task.CompletedTask;
        }

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
            throw new NotSupportedException();
        }

        public Task<bool> TryRotateAsync(
            UserSession currentSession,
            UserSession replacementSession,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task RevokeFamilyAsync(
            Guid tokenFamilyId,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<bool> RevokeAsync(
            string refreshTokenHash,
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
    }
}