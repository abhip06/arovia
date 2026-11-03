using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Application.Abstractions.Security;
using HealthcareStore.Application.Common.Exceptions;

namespace HealthcareStore.Application.Auth.Password;

public sealed class ResetPasswordService(
    IPasswordResetRepository passwordResetRepository,
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    ITokenGenerator tokenGenerator)
{
    public async Task ResetAsync(
        ResetPasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            request.Token,
            nameof(request.Token));

        ArgumentException.ThrowIfNullOrWhiteSpace(
            request.NewPassword,
            nameof(request.NewPassword));

        var tokenHash =
            tokenGenerator.Hash(request.Token);

        var result =
            await passwordResetRepository.FindAsync(
                tokenHash,
                cancellationToken);

        if (result is null)
        {
            throw new UnauthorizedException(
                "Invalid or expired password reset token.");
        }

        var (user, token) = result.Value;

        var now = DateTimeOffset.UtcNow;

        if (token.UsedAt.HasValue ||
            token.RevokedAt.HasValue ||
            token.ExpiresAt <= now)
        {
            throw new UnauthorizedException(
                "Invalid or expired password reset token.");
        }

        var credential =
            await userRepository.FindPasswordCredentialByUserIdAsync(
                user.Id,
                cancellationToken);

        if (credential is null)
        {
            throw new NotFoundException(
                "Password credentials were not found.");
        }

        if (passwordHasher.Verify(
                request.NewPassword,
                credential.HashedPassword))
        {
            throw new ConflictException(
                "New password must be different from the current password.");
        }

        var newPasswordHash =
            passwordHasher.Hash(
                request.NewPassword);

        var completed =
            await passwordResetRepository.TryCompleteAsync(
                user,
                credential,
                token,
                newPasswordHash,
                cancellationToken);

        if (!completed)
        {
            throw new UnauthorizedException(
                "Invalid or expired password reset token.");
        }
    }
}