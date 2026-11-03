using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Application.Abstractions.Security;
using HealthcareStore.Application.Common.Exceptions;

namespace HealthcareStore.Application.Auth.Password;

public sealed class ChangePasswordService(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IUserSessionRepository sessionRepository)
{
    public async Task ChangeAsync(
        ChangePasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.UserId == Guid.Empty)
        {
            throw new ArgumentException(
                "User ID cannot be empty.",
                nameof(request.UserId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(
            request.CurrentPassword,
            nameof(request.CurrentPassword));

        ArgumentException.ThrowIfNullOrWhiteSpace(
            request.NewPassword,
            nameof(request.NewPassword));

        var credential =
            await userRepository.FindPasswordCredentialByUserIdAsync(
                request.UserId,
                cancellationToken);

        if (credential is null)
        {
            throw new NotFoundException(
                "Password credentials were not found.");
        }

        if (!passwordHasher.Verify(
                request.CurrentPassword,
                credential.HashedPassword))
        {
            throw new UnauthorizedException(
                "Current password is incorrect.");
        }

        if (request.CurrentPassword ==
            request.NewPassword)
        {
            throw new ConflictException(
                "New password must be different from the current password.");
        }

        var newPasswordHash =
            passwordHasher.Hash(request.NewPassword);

        credential.ChangePassword(newPasswordHash);

        await userRepository.SaveChangesAsync(cancellationToken);

        await sessionRepository.RevokeAllAsync(request.UserId, cancellationToken);
    }
}