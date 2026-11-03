using HealthcareStore.Application.Abstractions.Communication;
using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Application.Abstractions.Security;
using HealthcareStore.Application.Auth.ResendEmailVerification;
using HealthcareStore.Application.Users.GetAll;
using HealthcareStore.Domain.Auth;
using HealthcareStore.Domain.Users;

namespace HealthcareStore.Application.Tests.Auth.ResendEmailVerification;

public sealed class ResendEmailVerificationServiceTests
{
    [Fact]
    public async Task ResendAsync_ShouldCreateNewTokenAndSendEmail_WhenUserIsUnverified()
    {
        var user = User.Create(
            "John",
            "Doe",
            "john@example.com");

        var userRepository = new FakeUserRepository
        {
            FindByEmailResult = user
        };

        var verificationRepository = new FakeEmailVerificationRepository();
        var tokenGenerator = new FakeVerificationTokenGenerator();
        var emailSender = new FakeEmailSender();

        var service = new ResendEmailVerificationService(
            userRepository,
            verificationRepository,
            tokenGenerator,
            emailSender);

        await service.ResendAsync(
            new ResendEmailVerificationRequest(
                " JOHN@EXAMPLE.COM "));

        Assert.Equal(
            "john@example.com",
            userRepository.FindByEmailArgument);

        Assert.Equal(
            user.Id,
            verificationRepository.ReplacedUserId);

        Assert.NotNull(
            verificationRepository.ReplacedToken);

        Assert.Equal(
            "HASHED:VERIFICATION-TOKEN",
            verificationRepository.ReplacedToken!.TokenHash);

        Assert.Equal(
            "john@example.com",
            emailSender.Email);

        Assert.Equal(
            "John",
            emailSender.FirstName);

        Assert.Equal(
            "VERIFICATION-TOKEN",
            emailSender.VerificationToken);

        Assert.Equal(1, tokenGenerator.GenerateCallCount);
        Assert.Equal(1, emailSender.SendCallCount);
    }

    [Fact]
    public async Task ResendAsync_ShouldDoNothing_WhenUserDoesNotExist()
    {
        var userRepository = new FakeUserRepository();
        var verificationRepository = new FakeEmailVerificationRepository();
        var tokenGenerator = new FakeVerificationTokenGenerator();
        var emailSender = new FakeEmailSender();

        var service = new ResendEmailVerificationService(
            userRepository,
            verificationRepository,
            tokenGenerator,
            emailSender);

        await service.ResendAsync(
            new ResendEmailVerificationRequest(
                "unknown@example.com"));

        Assert.Equal(
            "unknown@example.com",
            userRepository.FindByEmailArgument);

        Assert.Null(verificationRepository.ReplacedToken);
        Assert.Equal(0, tokenGenerator.GenerateCallCount);
        Assert.Equal(0, emailSender.SendCallCount);
    }

    [Fact]
    public async Task ResendAsync_ShouldDoNothing_WhenUserIsAlreadyVerified()
    {
        var user = User.Create(
            "John",
            "Doe",
            "john@example.com");

        user.VerifyEmail();

        var userRepository = new FakeUserRepository
        {
            FindByEmailResult = user
        };

        var verificationRepository = new FakeEmailVerificationRepository();
        var tokenGenerator = new FakeVerificationTokenGenerator();
        var emailSender = new FakeEmailSender();

        var service = new ResendEmailVerificationService(
            userRepository,
            verificationRepository,
            tokenGenerator,
            emailSender);

        await service.ResendAsync(
            new ResendEmailVerificationRequest(
                user.Email));

        Assert.Null(verificationRepository.ReplacedToken);
        Assert.Equal(0, tokenGenerator.GenerateCallCount);
        Assert.Equal(0, emailSender.SendCallCount);
    }

    [Fact]
    public async Task ResendAsync_ShouldNotSendEmail_WhenTokenReplacementFails()
    {
        var user = User.Create(
            "John",
            "Doe",
            "john@example.com");

        var userRepository = new FakeUserRepository
        {
            FindByEmailResult = user
        };

        var verificationRepository = new FakeEmailVerificationRepository
        {
            ReplaceResult = false
        };

        var tokenGenerator = new FakeVerificationTokenGenerator();
        var emailSender = new FakeEmailSender();

        var service = new ResendEmailVerificationService(
            userRepository,
            verificationRepository,
            tokenGenerator,
            emailSender);

        await service.ResendAsync(
            new ResendEmailVerificationRequest(
                user.Email));

        Assert.Equal(1, tokenGenerator.GenerateCallCount);
        Assert.Equal(0, emailSender.SendCallCount);

        Assert.Equal(
            user.Id,
            verificationRepository.ReplacedUserId);

        Assert.NotNull(
            verificationRepository.ReplacedToken);
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        public User? FindByEmailResult { get; set; }

        public string? FindByEmailArgument { get; private set; }

        public bool EmailExists { get; set; }

        public bool PhoneExists { get; set; }

        public User? User { get; private set; }

        public PasswordCredential? Credential { get; private set; }

        public EmailVerificationToken? EmailVerificationToken { get; private set; }

        public int AddCallCount { get; private set; }

        public (User User, PasswordCredential Credential)? FindForAuthenticationResult { get; set; }

        public string? FindForAuthenticationArgument { get; set; }

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

        public Task<GetAllUsersDto> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
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

    private sealed class FakeEmailVerificationRepository
    : IEmailVerificationRepository
    {
        public Guid? ReplacedUserId { get; private set; }

        public EmailVerificationToken? ReplacedToken { get; private set; }

        public bool ReplaceResult { get; set; } = true;

        public Task<(User User, EmailVerificationToken Token)?> FindAsync(
            string tokenHash,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<bool> TryCompleteAsync(
            User user,
            EmailVerificationToken token,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<bool> ReplaceAsync(
            Guid userId,
            EmailVerificationToken newToken,
            CancellationToken cancellationToken = default)
        {
            ReplacedUserId = userId;
            ReplacedToken = newToken;

            return Task.FromResult(ReplaceResult);
        }
    }

    private sealed class FakeVerificationTokenGenerator
        : ITokenGenerator
    {
        public int GenerateCallCount { get; private set; }

        public string Generate()
        {
            GenerateCallCount++;

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