using HealthcareStore.Application.Abstractions.Communication;
using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Application.Abstractions.Security;
using HealthcareStore.Domain.Auth;

namespace HealthcareStore.Application.Auth.Password;

public sealed class RequestPasswordResetService(
    IUserRepository userRepository,
    IPasswordResetRepository passwordResetRepository,
    ITokenGenerator tokenGenerator,
    IEmailSender emailSender)
{
    private const int TokenLifetimeHours = 1;

    public async Task RequestAsync(
        RequestPasswordResetRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            request.Email,
            nameof(request.Email));

        var email = request.Email.Trim();

        var user =
            await userRepository.FindByEmailAsync(
                email,
                cancellationToken);

        if (user is null)
        {
            return;
        }

        var rawToken =
            tokenGenerator.Generate();

        var tokenHash =
            tokenGenerator.Hash(rawToken);

        var token =
            PasswordResetToken.Create(
                user.Id,
                tokenHash,
                DateTimeOffset.UtcNow.AddHours(
                    TokenLifetimeHours));

        var replaced =
            await passwordResetRepository.ReplaceAsync(
                user.Id,
                token,
                cancellationToken);

        if (!replaced)
        {
            return;
        }

        await emailSender.SendPasswordResetAsync(
            user.Email,
            user.FirstName,
            rawToken,
            cancellationToken);
    }
}