using HealthcareStore.Application.Abstractions.Communication;
using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Application.Abstractions.Security;
using HealthcareStore.Application.Auth.Register;
using HealthcareStore.Application.Common.Exceptions;
using HealthcareStore.Application.Users.GetAll;
using HealthcareStore.Domain.Auth;
using HealthcareStore.Domain.Users;

namespace HealthcareStore.Application.Tests.Auth.Register;

public sealed class RegisterUserServiceTests
{
    [Fact]
    public async Task RegisterAsync_ShouldCreateCustomerWithCredentialsAndVerificationToken_WhenRequestIsValid()
    {
        var repository = new FakeUserRepository();
        var passwordHasher = new FakePasswordHasher();
        var verificationTokenGenerator = new FakeVerificationTokenGenerator();
        var emailSender = new FakeEmailSender();

        var service = new RegisterUserService(
            repository,
            passwordHasher,
            verificationTokenGenerator,
            emailSender);

        var request = new RegisterUserRequest(
            "John",
            "Doe",
            "john@example.com",
            "Password123!");

        var response = await service.RegisterAsync(request);

        Assert.NotEqual(Guid.Empty, response.Id);
        Assert.Equal("John", response.FirstName);
        Assert.Equal("Doe", response.LastName);
        Assert.Equal("john@example.com", response.Email);
        Assert.Equal("Customer", response.Role);
        Assert.False(response.IsEmailVerified);

        Assert.NotNull(repository.User);
        Assert.NotNull(repository.Credential);
        Assert.NotNull(repository.EmailVerificationToken);

        Assert.Equal(response.Id, repository.User!.Id);
        Assert.Equal(repository.User.Id, repository.Credential!.UserId);
        Assert.Equal(
            repository.User.Id,
            repository.EmailVerificationToken!.UserId);

        Assert.Equal(1, repository.AddCallCount);
    }

    [Fact]
    public async Task RegisterAsync_ShouldNormalizeEmail()
    {
        var repository = new FakeUserRepository();
        var passwordHasher = new FakePasswordHasher();
        var verificationTokenGenerator = new FakeVerificationTokenGenerator();
        var emailSender = new FakeEmailSender();

        var service = new RegisterUserService(
            repository,
            passwordHasher,
            verificationTokenGenerator,
            emailSender);

        var request = new RegisterUserRequest(
            "John",
            "Doe",
            "  JOHN@EXAMPLE.COM  ",
            "Password123!");

        var response = await service.RegisterAsync(request);

        Assert.Equal("john@example.com", response.Email);
        Assert.Equal("john@example.com", repository.User!.Email);
        Assert.Equal("john@example.com", emailSender.Email);
    }

    [Fact]
    public async Task RegisterAsync_ShouldHashPassword()
    {
        var repository = new FakeUserRepository();
        var passwordHasher = new FakePasswordHasher();
        var verificationTokenGenerator = new FakeVerificationTokenGenerator();
        var emailSender = new FakeEmailSender();

        var service = new RegisterUserService(
            repository,
            passwordHasher,
            verificationTokenGenerator,
            emailSender);

        var request = new RegisterUserRequest(
            "John",
            "Doe",
            "john@example.com",
            "Password123!");

        await service.RegisterAsync(request);

        Assert.Equal(
            "HASHED:Password123!",
            repository.Credential!.HashedPassword);

        Assert.Equal(
            "Password123!",
            passwordHasher.LastPassword);
    }

    [Fact]
    public async Task RegisterAsync_ShouldAssignCustomerRole()
    {
        var repository = new FakeUserRepository();
        var passwordHasher = new FakePasswordHasher();
        var verificationTokenGenerator = new FakeVerificationTokenGenerator();
        var emailSender = new FakeEmailSender();

        var service = new RegisterUserService(
            repository,
            passwordHasher,
            verificationTokenGenerator,
            emailSender);

        var request = new RegisterUserRequest(
            "John",
            "Doe",
            "john@example.com",
            "Password123!");

        await service.RegisterAsync(request);

        Assert.Equal(
            UserRole.Customer,
            repository.User!.Role);
    }

    [Fact]
    public async Task RegisterAsync_ShouldCreateCredentialForCreatedUser()
    {
        var repository = new FakeUserRepository();
        var passwordHasher = new FakePasswordHasher();
        var verificationTokenGenerator = new FakeVerificationTokenGenerator();
        var emailSender = new FakeEmailSender();

        var service = new RegisterUserService(
            repository,
            passwordHasher,
            verificationTokenGenerator,
            emailSender);

        var request = new RegisterUserRequest(
            "John",
            "Doe",
            "john@example.com",
            "Password123!");

        await service.RegisterAsync(request);

        Assert.NotNull(repository.User);
        Assert.NotNull(repository.Credential);

        Assert.Equal(
            repository.User!.Id,
            repository.Credential!.UserId);
    }

    [Fact]
    public async Task RegisterAsync_ShouldCreateVerificationTokenForCreatedUser()
    {
        var repository = new FakeUserRepository();
        var passwordHasher = new FakePasswordHasher();
        var verificationTokenGenerator = new FakeVerificationTokenGenerator();
        var emailSender = new FakeEmailSender();

        var service = new RegisterUserService(
            repository,
            passwordHasher,
            verificationTokenGenerator,
            emailSender);

        var request = new RegisterUserRequest(
            "John",
            "Doe",
            "john@example.com",
            "Password123!");

        await service.RegisterAsync(request);

        Assert.NotNull(repository.User);
        Assert.NotNull(repository.EmailVerificationToken);

        Assert.Equal(
            repository.User!.Id,
            repository.EmailVerificationToken!.UserId);

        Assert.Equal(
            "HASHED:VERIFICATION-TOKEN",
            repository.EmailVerificationToken.TokenHash);
    }

    [Fact]
    public async Task RegisterAsync_ShouldSendRawVerificationTokenByEmail()
    {
        var repository = new FakeUserRepository();
        var passwordHasher = new FakePasswordHasher();
        var verificationTokenGenerator = new FakeVerificationTokenGenerator();
        var emailSender = new FakeEmailSender();

        var service = new RegisterUserService(
            repository,
            passwordHasher,
            verificationTokenGenerator,
            emailSender);

        var request = new RegisterUserRequest(
            "John",
            "Doe",
            "john@example.com",
            "Password123!");

        await service.RegisterAsync(request);

        Assert.Equal(1, emailSender.SendCallCount);
        Assert.Equal(
            "john@example.com",
            emailSender.Email);
        Assert.Equal(
            "John",
            emailSender.FirstName);
        Assert.Equal(
            "VERIFICATION-TOKEN",
            emailSender.VerificationToken);
    }

    [Fact]
    public async Task RegisterAsync_ShouldPersistOnlyVerificationTokenHash()
    {
        var repository = new FakeUserRepository();
        var passwordHasher = new FakePasswordHasher();
        var verificationTokenGenerator = new FakeVerificationTokenGenerator();
        var emailSender = new FakeEmailSender();

        var service = new RegisterUserService(
            repository,
            passwordHasher,
            verificationTokenGenerator,
            emailSender);

        var request = new RegisterUserRequest(
            "John",
            "Doe",
            "john@example.com",
            "Password123!");

        await service.RegisterAsync(request);

        Assert.Equal(
            "VERIFICATION-TOKEN",
            emailSender.VerificationToken);

        Assert.Equal(
            "HASHED:VERIFICATION-TOKEN",
            repository.EmailVerificationToken!.TokenHash);

        Assert.NotEqual(
            emailSender.VerificationToken,
            repository.EmailVerificationToken.TokenHash);
    }

    [Fact]
    public async Task RegisterAsync_ShouldRejectDuplicateEmail()
    {
        var repository = new FakeUserRepository
        {
            EmailExists = true
        };

        var passwordHasher = new FakePasswordHasher();
        var verificationTokenGenerator = new FakeVerificationTokenGenerator();
        var emailSender = new FakeEmailSender();

        var service = new RegisterUserService(
            repository,
            passwordHasher,
            verificationTokenGenerator,
            emailSender);

        var request = new RegisterUserRequest(
            "John",
            "Doe",
            "john@example.com",
            "Password123!");

        var exception = await Assert.ThrowsAsync<ConflictException>(
            () => service.RegisterAsync(request));

        Assert.Equal(
            "A user with this email already exists.",
            exception.Message);

        Assert.Null(repository.User);
        Assert.Null(repository.Credential);
        Assert.Null(repository.EmailVerificationToken);
        Assert.Equal(0, repository.AddCallCount);
        Assert.Equal(0, emailSender.SendCallCount);
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        public bool EmailExists { get; set; }

        public bool PhoneExists { get; set; }

        public User? FindByEmailResult { get; set; }

        public string? FindByEmailArgument { get; private set; }

        public User? User { get; private set; }

        public PasswordCredential? Credential { get; private set; }

        public EmailVerificationToken? EmailVerificationToken { get; private set; }

        public int AddCallCount { get; private set; }

        public (User User, PasswordCredential Credential)? FindForAuthenticationResult { get; set; }

        public string? FindForAuthenticationArgument { get; private set; }

        public Task<bool> ExistsByEmailAsync(
            string email,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(EmailExists);
        }

        public Task<bool> ExistsByPhoneAsync(
            string phone,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(PhoneExists);
        }

        public Task<User?> FindByEmailAsync(
            string email,
            CancellationToken cancellationToken = default)
        {
            FindByEmailArgument = email;

            return Task.FromResult(FindByEmailResult);
        }

        public Task<string?> GetPhoneAsync(Guid userId, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public Task<(User User, PasswordCredential Credential)?> FindForAuthenticationAsync(
            string email,
            CancellationToken cancellationToken = default)
        {
            FindForAuthenticationArgument = email;

            return Task.FromResult(FindForAuthenticationResult);
        }

        public Task<GetAllUsersDto> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task AddAsync(
            User user,
            PasswordCredential credential,
            EmailVerificationToken emailVerificationToken,
            CancellationToken cancellationToken = default)
        {
            User = user;
            Credential = credential;
            EmailVerificationToken = emailVerificationToken;
            AddCallCount++;

            return Task.CompletedTask;
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

        public Task<PasswordCredential?> FindPasswordCredentialByUserIdAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }
    }

    private sealed class FakePasswordHasher : IPasswordHasher
    {
        public string? LastPassword { get; private set; }

        public string Hash(string password)
        {
            LastPassword = password;
            return $"HASHED:{password}";
        }

        public bool Verify(string password, string passwordHash)
        {
            return passwordHash == $"HASHED:{password}";
        }
    }

    private sealed class FakeVerificationTokenGenerator
        : ITokenGenerator
    {
        public string Generate()
        {
            return "VERIFICATION-TOKEN";
        }

        public string Hash(string token)
        {
            return $"HASHED:{token}";
        }
    }

    private sealed class FakeEmailSender : IEmailSender
    {
        public string? Email { get; private set; }

        public string? FirstName { get; private set; }

        public User? FindByEmailResult { get; set; }

        public string? FindByEmailArgument { get; private set; }

        public string? VerificationToken { get; private set; }

        public int SendCallCount { get; private set; }

        public Task SendEmailVerificationAsync(
            string email,
            string firstName,
            string verificationToken,
            CancellationToken cancellationToken = default)
        {
            Email = email;
            FirstName = firstName;
            VerificationToken = verificationToken;
            SendCallCount++;

            return Task.CompletedTask;
        }

        public Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            FindByEmailArgument = email;

            return Task.FromResult(FindByEmailResult);
        }

        public Task SendPasswordResetAsync(
            string email,
            string firstName,
            string resetToken,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task SendOrderPaymentConfirmationAsync(
            string email,
            OrderPaymentConfirmationEmail confirmation,
            Stream receiptPdf,
            string receiptFileName,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }
    }
}
