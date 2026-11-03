using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Application.Abstractions.Security;
using HealthcareStore.Application.Auth.Password;
using HealthcareStore.Application.Common.Exceptions;
using HealthcareStore.Application.Users.GetAll;
using HealthcareStore.Domain.Auth;
using HealthcareStore.Domain.Users;

namespace HealthcareStore.Application.Tests.Auth.Password;

public sealed class ResetPasswordServiceTests
{
    [Fact]
    public async Task ResetAsync_ShouldResetPassword_WhenTokenIsValid()
    {
        var user = User.Create(
            "John",
            "Doe",
            "john@example.com");

        var credential = PasswordCredential.Create(
            user.Id,
            "HASHED:OldPassword123!");

        var token = PasswordResetToken.Create(
            user.Id,
            "HASHED:RESET-TOKEN",
            DateTimeOffset.UtcNow.AddHours(1));

        var repository = new FakePasswordResetRepository
        {
            Result = (user, token),
            CompleteResult = true
        };

        var userRepository = new FakeUserRepository
        {
            PasswordCredential = credential
        };

        var passwordHasher = new FakePasswordHasher();

        var service = new ResetPasswordService(
            repository,
            userRepository,
            passwordHasher,
            new FakeTokenGenerator());

        await service.ResetAsync(
            new ResetPasswordRequest(
                "RESET-TOKEN",
                "NewPassword123!"));

        Assert.Equal(
            "HASHED:NewPassword123!",
            credential.HashedPassword);

        Assert.Equal(
            "HASHED:RESET-TOKEN",
            repository.TokenHashArgument);

        Assert.Equal(
            "HASHED:NewPassword123!",
            repository.NewPasswordHashArgument);

        Assert.Equal(
            1,
            repository.TryCompleteCallCount);
    }

    [Fact]
    public async Task ResetAsync_ShouldRejectUnknownToken()
    {
        var repository = new FakePasswordResetRepository
        {
            Result = null
        };

        var service = new ResetPasswordService(
            repository,
            new FakeUserRepository(),
            new FakePasswordHasher(),
            new FakeTokenGenerator());

        var exception =
            await Assert.ThrowsAsync<UnauthorizedException>(
                () => service.ResetAsync(
                    new ResetPasswordRequest(
                        "INVALID-TOKEN",
                        "NewPassword123!")));

        Assert.Equal(
            "Invalid or expired password reset token.",
            exception.Message);

        Assert.Equal(
            0,
            repository.TryCompleteCallCount);
    }

    [Fact]
    public async Task ResetAsync_ShouldRejectUsedToken()
    {
        var user = User.Create(
            "John",
            "Doe",
            "john@example.com");

        var token = PasswordResetToken.Create(
            user.Id,
            "HASHED:RESET-TOKEN",
            DateTimeOffset.UtcNow.AddHours(1));

        token.MarkUsed();

        var repository = new FakePasswordResetRepository
        {
            Result = (user, token)
        };

        var service = new ResetPasswordService(
            repository,
            new FakeUserRepository(),
            new FakePasswordHasher(),
            new FakeTokenGenerator());

        await Assert.ThrowsAsync<UnauthorizedException>(
            () => service.ResetAsync(
                new ResetPasswordRequest(
                    "RESET-TOKEN",
                    "NewPassword123!")));

        Assert.Equal(
            0,
            repository.TryCompleteCallCount);
    }

    [Fact]
    public async Task ResetAsync_ShouldRejectExpiredToken()
    {
        var user = User.Create(
            "John",
            "Doe",
            "john@example.com");

        var token = PasswordResetToken.Create(
            user.Id,
            "HASHED:RESET-TOKEN",
            DateTimeOffset.UtcNow.AddMinutes(1));

        var repository = new FakePasswordResetRepository
        {
            Result = (user, token)
        };

        repository.ForceExpiration(token);

        var service = new ResetPasswordService(
            repository,
            new FakeUserRepository(),
            new FakePasswordHasher(),
            new FakeTokenGenerator());

        await Assert.ThrowsAsync<UnauthorizedException>(
            () => service.ResetAsync(
                new ResetPasswordRequest(
                    "RESET-TOKEN",
                    "NewPassword123!")));

        Assert.Equal(
            0,
            repository.TryCompleteCallCount);
    }

    [Fact]
    public async Task ResetAsync_ShouldRejectRevokedToken()
    {
        var user = User.Create(
            "John",
            "Doe",
            "john@example.com");

        var token = PasswordResetToken.Create(
            user.Id,
            "HASHED:RESET-TOKEN",
            DateTimeOffset.UtcNow.AddHours(1));

        token.Revoke();

        var repository = new FakePasswordResetRepository
        {
            Result = (user, token)
        };

        var service = new ResetPasswordService(
            repository,
            new FakeUserRepository(),
            new FakePasswordHasher(),
            new FakeTokenGenerator());

        await Assert.ThrowsAsync<UnauthorizedException>(
            () => service.ResetAsync(
                new ResetPasswordRequest(
                    "RESET-TOKEN",
                    "NewPassword123!")));

        Assert.Equal(
            0,
            repository.TryCompleteCallCount);
    }

    [Fact]
    public async Task ResetAsync_ShouldRejectMissingPasswordCredential()
    {
        var user = User.Create(
            "John",
            "Doe",
            "john@example.com");

        var token = PasswordResetToken.Create(
            user.Id,
            "HASHED:RESET-TOKEN",
            DateTimeOffset.UtcNow.AddHours(1));

        var repository = new FakePasswordResetRepository
        {
            Result = (user, token)
        };

        var service = new ResetPasswordService(
            repository,
            new FakeUserRepository(),
            new FakePasswordHasher(),
            new FakeTokenGenerator());

        var exception =
            await Assert.ThrowsAsync<NotFoundException>(
                () => service.ResetAsync(
                    new ResetPasswordRequest(
                        "RESET-TOKEN",
                        "NewPassword123!")));

        Assert.Equal(
            "Password credentials were not found.",
            exception.Message);
    }

    [Fact]
    public async Task ResetAsync_ShouldRejectSamePassword()
    {
        var user = User.Create(
            "John",
            "Doe",
            "john@example.com");

        var credential = PasswordCredential.Create(
            user.Id,
            "HASHED:OldPassword123!");

        var token = PasswordResetToken.Create(
            user.Id,
            "HASHED:RESET-TOKEN",
            DateTimeOffset.UtcNow.AddHours(1));

        var repository = new FakePasswordResetRepository
        {
            Result = (user, token)
        };

        var service = new ResetPasswordService(
            repository,
            new FakeUserRepository
            {
                PasswordCredential = credential
            },
            new FakePasswordHasher(),
            new FakeTokenGenerator());

        var exception =
            await Assert.ThrowsAsync<ConflictException>(
                () => service.ResetAsync(
                    new ResetPasswordRequest(
                        "RESET-TOKEN",
                        "OldPassword123!")));

        Assert.Equal(
            "New password must be different from the current password.",
            exception.Message);

        Assert.Equal(
            0,
            repository.TryCompleteCallCount);
    }

    [Fact]
    public async Task ResetAsync_ShouldRejectMissingRequest()
    {
        var service = new ResetPasswordService(
            new FakePasswordResetRepository(),
            new FakeUserRepository(),
            new FakePasswordHasher(),
            new FakeTokenGenerator());

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => service.ResetAsync(null!));
    }

    private sealed class FakePasswordResetRepository
        : IPasswordResetRepository
    {
        public (
            User User,
            PasswordResetToken Token)? Result
        {
            get;
            set;
        }

        public bool CompleteResult { get; set; }

        public string? TokenHashArgument { get; private set; }

        public string? NewPasswordHashArgument { get; private set; }

        public int TryCompleteCallCount { get; private set; }

        public Task<(User User, PasswordResetToken Token)?>
            FindAsync(
                string tokenHash,
                CancellationToken cancellationToken = default)
        {
            TokenHashArgument = tokenHash;

            return Task.FromResult(Result);
        }

        public Task<bool> TryCompleteAsync(
            User user,
            PasswordCredential credential,
            PasswordResetToken token,
            string newPasswordHash,
            CancellationToken cancellationToken = default)
        {
            NewPasswordHashArgument = newPasswordHash;
            TryCompleteCallCount++;

            if (CompleteResult)
            {
                credential.ChangePassword(newPasswordHash);
            }

            return Task.FromResult(CompleteResult);
        }

        public Task<bool> ReplaceAsync(
            Guid userId,
            PasswordResetToken newToken,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public void ForceExpiration(
            PasswordResetToken token)
        {
            var property =
                typeof(PasswordResetToken).GetProperty(
                    nameof(PasswordResetToken.ExpiresAt));

            property!.SetValue(
                token,
                DateTimeOffset.UtcNow.AddMinutes(-1));
        }
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        public PasswordCredential? PasswordCredential { get; set; }

        public Task<PasswordCredential?>
            FindPasswordCredentialByUserIdAsync(
                Guid userId,
                CancellationToken cancellationToken = default)
        {
            return Task.FromResult(PasswordCredential);
        }

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

        public Task<GetAllUsersDto> GetAllAsync(
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
            throw new NotSupportedException();
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

    private sealed class FakeTokenGenerator : ITokenGenerator
    {
        public string Generate()
        {
            return "RAW-RESET-TOKEN";
        }

        public string Hash(string token)
        {
            return $"HASHED:{token}";
        }
    }
}