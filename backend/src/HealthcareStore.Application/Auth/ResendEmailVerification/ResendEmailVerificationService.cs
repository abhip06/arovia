using HealthcareStore.Application.Abstractions.Communication;
using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Application.Abstractions.Security;
using HealthcareStore.Domain.Auth;

namespace HealthcareStore.Application.Auth.ResendEmailVerification;

public sealed class ResendEmailVerificationService
{
    private readonly IUserRepository _userRepository;
    private readonly IEmailVerificationRepository _emailVerificationRepository;
    private readonly ITokenGenerator _tokenGenerator;
    private readonly IEmailSender _emailSender;

    public ResendEmailVerificationService(
        IUserRepository userRepository,
        IEmailVerificationRepository emailVerificationRepository,
        ITokenGenerator tokenGenerator,
        IEmailSender emailSender)
    {
        ArgumentNullException.ThrowIfNull(userRepository);
        ArgumentNullException.ThrowIfNull(emailVerificationRepository);
        ArgumentNullException.ThrowIfNull(tokenGenerator);
        ArgumentNullException.ThrowIfNull(emailSender);

        _userRepository = userRepository;
        _emailVerificationRepository = emailVerificationRepository;
        _tokenGenerator = tokenGenerator;
        _emailSender = emailSender;
    }

    public async Task ResendAsync(
        ResendEmailVerificationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var email = request.Email.Trim().ToLowerInvariant();

        var user = await _userRepository.FindByEmailAsync(
            email,
            cancellationToken);

        if (user is null || user.IsEmailVerified)
        {
            return;
        }

        var verificationToken = _tokenGenerator.Generate();

        var verificationTokenHash = _tokenGenerator.Hash(
            verificationToken);

        var emailVerificationToken = EmailVerificationToken.Create(
            user.Id,
            verificationTokenHash,
            DateTimeOffset.UtcNow.AddHours(24));

        var replaced = await _emailVerificationRepository.ReplaceAsync(
            user.Id,
            emailVerificationToken,
            cancellationToken);

        if (!replaced)
        {
            return;
        }

        await _emailSender.SendEmailVerificationAsync(
            user.Email,
            user.FirstName,
            verificationToken,
            cancellationToken);
    }
}