using HealthcareStore.Domain.Auth;
using HealthcareStore.Domain.Users;

namespace HealthcareStore.Application.Abstractions.Persistence;

public interface IUserSessionRepository
{
    Task AddAsync(
        UserSession session,
        CancellationToken cancellationToken = default);

    Task<(User User, UserSession Session)?> FindForRefreshAsync(
        string refreshTokenHash,
        CancellationToken cancellationToken = default);

    Task<bool> TryRotateAsync(
        UserSession currentSession,
        UserSession replacementSession,
        CancellationToken cancellationToken = default);

    Task RevokeFamilyAsync(
        Guid tokenFamilyId,
        CancellationToken cancellationToken = default);

    Task<bool> RevokeAsync(
        string refreshTokenHash,
        CancellationToken cancellationToken = default);

    Task RevokeAllAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}