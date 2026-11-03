using HealthcareStore.Domain.Auth;
using HealthcareStore.Domain.Users;

namespace HealthcareStore.Application.Abstractions.Persistence;

public interface IPasswordResetRepository
{
    Task<(User User, PasswordResetToken Token)?> FindAsync(
        string tokenHash,
        CancellationToken cancellationToken = default);

    Task<bool> TryCompleteAsync(
        User user,
        PasswordCredential credential,
        PasswordResetToken token,
        string newPasswordHash,
        CancellationToken cancellationToken = default);

    Task<bool> ReplaceAsync(
        Guid userId,
        PasswordResetToken newToken,
        CancellationToken cancellationToken = default);
}