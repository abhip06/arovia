using HealthcareStore.Application.Users.GetAll;
using HealthcareStore.Domain.Auth;
using HealthcareStore.Domain.Users;

namespace HealthcareStore.Application.Abstractions.Persistence;

public interface IUserRepository
{
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<bool> ExistsByPhoneAsync(string phone, CancellationToken cancellationToken = default);

    Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<User?> FindByIdAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<GetAllUsersDto> GetAllAsync(CancellationToken cancellationToken = default);

    Task<string?> GetPhoneAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);

    Task<(User User, PasswordCredential Credential)?> FindForAuthenticationAsync(
        string email,
        CancellationToken cancellationToken = default);

    Task<PasswordCredential?> FindPasswordCredentialByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task AddAsync(User user,
        PasswordCredential credential,
        EmailVerificationToken emailVerificationToken,
        CancellationToken cancellationToken = default);

    Task AddExternalUserAsync(
        User user,
        ExternalIdentity identity,
        CancellationToken cancellationToken = default);
}
