using HealthcareStore.Application.Abstractions.Authentication;
using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Application.Abstractions.Security;
using HealthcareStore.Domain.Auth;
using HealthcareStore.Domain.Users;

namespace HealthcareStore.Application.Auth.AuthSession;

public sealed class AuthenticationSessionService(
    IUserSessionRepository sessionRepository,
    IAccessTokenGenerator accessTokenGenerator,
    IRefreshTokenGenerator refreshTokenGenerator) : IAuthenticationSessionService
{
    private const int RefreshTokenLifetimeDays = 30;

    public async Task<AuthenticationSessionResponse> CreateAsync(
        User user,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);

        var refreshToken =
            refreshTokenGenerator.Generate();

        var refreshTokenHash =
            refreshTokenGenerator.Hash(refreshToken);

        var tokenFamilyId = Guid.NewGuid();

        var session = UserSession.Create(
            user.Id,
            refreshTokenHash,
            tokenFamilyId,
            DateTimeOffset.UtcNow.AddDays(
                RefreshTokenLifetimeDays));

        await sessionRepository.AddAsync(
            session,
            cancellationToken);

        var accessToken =
            accessTokenGenerator.Generate(user, session.Id);

        return new AuthenticationSessionResponse(
            accessToken.Token,
            accessToken.ExpiresIn,
            refreshToken,
            session.ExpiresAt);
    }
}