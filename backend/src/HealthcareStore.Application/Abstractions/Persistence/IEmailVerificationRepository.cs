using HealthcareStore.Domain.Auth;
using HealthcareStore.Domain.Users;

namespace HealthcareStore.Application.Abstractions.Persistence;

public interface IEmailVerificationRepository
{
    Task<(User User, EmailVerificationToken Token)?> FindAsync(
        string tokenHash,
        CancellationToken cancellationToken = default);

    Task<bool> TryCompleteAsync(
        User user,
        EmailVerificationToken token,
        CancellationToken cancellationToken = default);

    Task<bool> ReplaceAsync(
        Guid userId,
        EmailVerificationToken newToken,
        CancellationToken cancellationToken = default);
}