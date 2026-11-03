using HealthcareStore.Application.Abstractions.Communication;
using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Application.Abstractions.Security;
using HealthcareStore.Application.Auth.Password;
using HealthcareStore.Application.Users.GetAll;
using HealthcareStore.Domain.Auth;
using HealthcareStore.Domain.Users;

namespace HealthcareStore.Application.Tests.Auth.Password;

public sealed class RequestPasswordResetServiceTests
{
    [Fact]
    public async Task RequestAsync_ShouldCreateTokenAndSendEmail_WhenUserExists()
    {
        var user = User.Create(
            "John",
            "Doe",
            "john@example.com");

        var userRepository = new FakeUserRepository
        {
            User = user
        };

        var resetRepository = new FakePasswordResetRepository();

        var tokenGenerator = new FakeTokenGenerator();

        var emailSender = new FakeEmailSender();

        var service = new RequestPasswordResetService(
            userRepository,
            resetRepository,
            tokenGenerator,
            emailSender);

        await service.RequestAsync(
            new RequestPasswordResetRequest(
                " JOHN@EXAMPLE.COM "));

        Assert.Equal(
            "john@example.com",
            userRepository.RequestedEmail);

        Assert.NotNull(resetRepository.Token);

        Assert.Equal(
            user.Id,
            resetRepository.Token!.UserId);

        Assert.Equal(
            "RAW-RESET-TOKEN",
            emailSender.ResetToken);

        Assert.Equal(
            "john@example.com",
            emailSender.Email);

        Assert.Equal(
            "John",
            emailSender.FirstName);

        Assert.Equal(
            1,
            emailSender.SendCallCount);
    }

    [Fact]
    public async Task RequestAsync_ShouldNotSendEmail_WhenUserDoesNotExist()
    {
        var userRepository = new FakeUserRepository();

        var resetRepository =
            new FakePasswordResetRepository();

        var emailSender = new FakeEmailSender();

        var service = new RequestPasswordResetService(
            userRepository,
            resetRepository,
            new FakeTokenGenerator(),
            emailSender);

        await service.RequestAsync(
            new RequestPasswordResetRequest(
                "unknown@example.com"));

        Assert.Equal(
            0,
            emailSender.SendCallCount);

        Assert.Null(resetRepository.Token);
    }

    [Fact]
    public async Task RequestAsync_ShouldReplaceExistingResetToken()
    {
        var user = User.Create(
            "John",
            "Doe",
            "john@example.com");

        var resetRepository =
            new FakePasswordResetRepository();

        var service = new RequestPasswordResetService(
            new FakeUserRepository
            {
                User = user
            },
            resetRepository,
            new FakeTokenGenerator(),
            new FakeEmailSender());

        await service.RequestAsync(
            new RequestPasswordResetRequest(
                user.Email));

        await service.RequestAsync(
            new RequestPasswordResetRequest(
                user.Email));

        Assert.Equal(
            2,
            resetRepository.ReplaceCallCount);
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        public User? User { get; set; }

        public string? RequestedEmail { get; private set; }

        public Task<User?> FindByEmailAsync(
            string email,
            CancellationToken cancellationToken = default)
        {
            RequestedEmail = email.Trim().ToLowerInvariant();

            return Task.FromResult(User);
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

        public Task<PasswordCredential?>
            FindPasswordCredentialByUserIdAsync(
                Guid userId,
                CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }
    }

    private sealed class FakePasswordResetRepository
        : IPasswordResetRepository
    {
        public PasswordResetToken? Token { get; private set; }

        public int ReplaceCallCount { get; private set; }

        public Task<(User User, PasswordResetToken Token)?>
            FindAsync(
                string tokenHash,
                CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<bool> TryCompleteAsync(
            User user,
            PasswordCredential credential,
            PasswordResetToken token,
            string newPasswordHash,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<bool> ReplaceAsync(
            Guid userId,
            PasswordResetToken newToken,
            CancellationToken cancellationToken = default)
        {
            Token = newToken;
            ReplaceCallCount++;

            return Task.FromResult(true);
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

    private sealed class FakeEmailSender : IEmailSender
    {
        public string? Email { get; private set; }

        public string? FirstName { get; private set; }

        public string? ResetToken { get; private set; }

        public int SendCallCount { get; private set; }

        public Task SendEmailVerificationAsync(
            string email,
            string firstName,
            string verificationToken,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task SendPasswordResetAsync(
            string email,
            string firstName,
            string resetToken,
            CancellationToken cancellationToken = default)
        {
            Email = email;
            FirstName = firstName;
            ResetToken = resetToken;
            SendCallCount++;

            return Task.CompletedTask;
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