using HealthcareStore.Application.Abstractions.Authentication;
using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Application.Abstractions.Security;
using HealthcareStore.Application.Auth.AuthSession;
using HealthcareStore.Application.Auth.Login;
using HealthcareStore.Application.Common.Exceptions;
using HealthcareStore.Application.Users.GetAll;
using HealthcareStore.Domain.Auth;
using HealthcareStore.Domain.Users;

namespace HealthcareStore.Application.Tests.Auth.Login;

public sealed class LoginUserServiceTests
{
    [Fact]
    public async Task LoginAsync_ShouldReturnAuthenticationResult_WhenCredentialsAreValidAndEmailIsVerified()
    {
        var user = User.Create(
            "John",
            "Doe",
            "john@example.com");

        user.VerifyEmail();

        var credential = PasswordCredential.Create(
            user.Id,
            "HASHED:Password123!");

        var userRepository = new FakeUserRepository
        {
            AuthenticationResult = (user, credential)
        };

        var passwordHasher = new FakePasswordHasher();

        var authenticationSessionService =
            new FakeAuthenticationSessionService
            {
                Result = new AuthenticationSessionResponse(
                    "ACCESS-TOKEN",
                    900,
                    "REFRESH-TOKEN",
                    DateTimeOffset.UtcNow.AddDays(30))
            };

        var service = new LoginUserService(
            userRepository,
            passwordHasher,
            authenticationSessionService);

        var response = await service.LoginAsync(
            new LoginUserRequest(
                " JOHN@EXAMPLE.COM ",
                "Password123!"));

        Assert.Equal(
            "ACCESS-TOKEN",
            response.AccessToken);

        Assert.Equal(
            "REFRESH-TOKEN",
            response.RefreshToken);

        Assert.Equal(
            900,
            response.ExpiresIn);

        Assert.Equal(
            user.Id,
            response.UserId);

        Assert.Equal(
            "John",
            response.FirstName);

        Assert.Equal(
            "Doe",
            response.LastName);

        Assert.Equal(
            "john@example.com",
            response.Email);

        Assert.Equal(
            "Customer",
            response.Role);

        Assert.Equal(
            "john@example.com",
            userRepository.AuthenticationEmailArgument);

        Assert.Equal(
            "Password123!",
            passwordHasher.LastPassword);

        Assert.Equal(
            "HASHED:Password123!",
            passwordHasher.LastPasswordHash);

        Assert.Same(
            user,
            authenticationSessionService.LastUser);

        Assert.Equal(
            1,
            authenticationSessionService.CreateCallCount);
    }

    [Fact]
    public async Task LoginAsync_ShouldNormalizeEmail()
    {
        var user = User.Create(
            "John",
            "Doe",
            "john@example.com");

        user.VerifyEmail();

        var credential = PasswordCredential.Create(
            user.Id,
            "HASHED:Password123!");

        var userRepository = new FakeUserRepository
        {
            AuthenticationResult = (user, credential)
        };

        var service = new LoginUserService(
            userRepository,
            new FakePasswordHasher(),
            new FakeAuthenticationSessionService());

        await service.LoginAsync(
            new LoginUserRequest(
                " JOHN@EXAMPLE.COM ",
                "Password123!"));

        Assert.Equal(
            "john@example.com",
            userRepository.AuthenticationEmailArgument);
    }

    [Fact]
    public async Task LoginAsync_ShouldRejectUnknownEmail()
    {
        var userRepository = new FakeUserRepository
        {
            AuthenticationResult = null
        };

        var service = new LoginUserService(
            userRepository,
            new FakePasswordHasher(),
            new FakeAuthenticationSessionService());

        var exception =
            await Assert.ThrowsAsync<UnauthorizedException>(
                () => service.LoginAsync(
                    new LoginUserRequest(
                        "unknown@example.com",
                        "Password123!")));

        Assert.Equal(
            "Invalid email or password.",
            exception.Message);
    }

    [Fact]
    public async Task LoginAsync_ShouldRejectInvalidPassword()
    {
        var user = User.Create(
            "John",
            "Doe",
            "john@example.com");

        user.VerifyEmail();

        var credential = PasswordCredential.Create(
            user.Id,
            "HASHED:CorrectPassword");

        var userRepository = new FakeUserRepository
        {
            AuthenticationResult = (user, credential)
        };

        var authenticationSessionService =
            new FakeAuthenticationSessionService();

        var service = new LoginUserService(
            userRepository,
            new FakePasswordHasher(),
            authenticationSessionService);

        var exception =
            await Assert.ThrowsAsync<UnauthorizedException>(
                () => service.LoginAsync(
                    new LoginUserRequest(
                        "john@example.com",
                        "WrongPassword")));

        Assert.Equal(
            "Invalid email or password.",
            exception.Message);

        Assert.Equal(
            0,
            authenticationSessionService.CreateCallCount);
    }

    [Fact]
    public async Task LoginAsync_ShouldRejectUnverifiedEmail()
    {
        var user = User.Create(
            "John",
            "Doe",
            "john@example.com");

        var credential = PasswordCredential.Create(
            user.Id,
            "HASHED:Password123!");

        var userRepository = new FakeUserRepository
        {
            AuthenticationResult = (user, credential)
        };

        var authenticationSessionService =
            new FakeAuthenticationSessionService();

        var service = new LoginUserService(
            userRepository,
            new FakePasswordHasher(),
            authenticationSessionService);

        var exception =
            await Assert.ThrowsAsync<UnauthorizedException>(
                () => service.LoginAsync(
                    new LoginUserRequest(
                        "john@example.com",
                        "Password123!")));

        Assert.Equal(
            "Email address must be verified before signing in.",
            exception.Message);

        Assert.Equal(
            0,
            authenticationSessionService.CreateCallCount);
    }

    [Fact]
    public async Task LoginAsync_ShouldUseAuthenticationSessionService()
    {
        var user = User.Create(
            "John",
            "Doe",
            "john@example.com");

        user.VerifyEmail();

        var credential = PasswordCredential.Create(
            user.Id,
            "HASHED:Password123!");

        var authenticationSessionService =
            new FakeAuthenticationSessionService
            {
                Result = new AuthenticationSessionResponse(
                    "ACCESS-TOKEN",
                    900,
                    "REFRESH-TOKEN",
                    DateTimeOffset.UtcNow.AddDays(30))
            };

        var service = new LoginUserService(
            new FakeUserRepository
            {
                AuthenticationResult = (user, credential)
            },
            new FakePasswordHasher(),
            authenticationSessionService);

        await service.LoginAsync(
            new LoginUserRequest(
                user.Email,
                "Password123!"));

        Assert.Same(
            user,
            authenticationSessionService.LastUser);

        Assert.Equal(
            1,
            authenticationSessionService.CreateCallCount);
    }

    [Fact]
    public async Task LoginAsync_ShouldRejectMissingRequest()
    {
        var service = new LoginUserService(
            new FakeUserRepository(),
            new FakePasswordHasher(),
            new FakeAuthenticationSessionService());

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => service.LoginAsync(null!));
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        public (
            User User,
            PasswordCredential Credential)? AuthenticationResult
        {
            get;
            set;
        }

        public string? AuthenticationEmailArgument
        {
            get;
            private set;
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

        public Task<User?> FindByEmailAsync(
            string email,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }
        
        public Task<GetAllUsersDto> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<(User User, PasswordCredential Credential)?>
            FindForAuthenticationAsync(
                string email,
                CancellationToken cancellationToken = default)
        {
            AuthenticationEmailArgument = email;

            return Task.FromResult(
                AuthenticationResult);
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

        public Task<User?> FindByIdAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<PasswordCredential?>
            FindPasswordCredentialByUserIdAsync(
                Guid userId,
                CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<string?> GetPhoneAsync(Guid userId, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }
    }

    private sealed class FakePasswordHasher : IPasswordHasher
    {
        public string? LastPassword
        {
            get;
            private set;
        }

        public string? LastPasswordHash
        {
            get;
            private set;
        }

        public string Hash(string password)
        {
            throw new NotSupportedException();
        }

        public bool Verify(
            string password,
            string passwordHash)
        {
            LastPassword = password;
            LastPasswordHash = passwordHash;

            return passwordHash == $"HASHED:{password}";
        }
    }

    private sealed class FakeAuthenticationSessionService
        : IAuthenticationSessionService
    {
        public AuthenticationSessionResponse Result { get; set; } =
            new(
                "ACCESS-TOKEN",
                900,
                "REFRESH-TOKEN",
                DateTimeOffset.UtcNow.AddDays(30));

        public User? LastUser
        {
            get;
            private set;
        }

        public int CreateCallCount
        {
            get;
            private set;
        }

        public Task<AuthenticationSessionResponse> CreateAsync(
            User user,
            CancellationToken cancellationToken = default)
        {
            LastUser = user;
            CreateCallCount++;

            return Task.FromResult(Result);
        }
    }
}