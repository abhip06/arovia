using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Application.Abstractions.Security;
using HealthcareStore.Application.Common.Exceptions;
using HealthcareStore.Domain.Auth;

namespace HealthcareStore.Application.Auth.RefreshToken;

public sealed class RefreshTokenService
{
    private const int RefreshTokenLifetimeDays = 30;

    private readonly IUserSessionRepository _sessionRepository;
    private readonly IAccessTokenGenerator _accessTokenGenerator;
    private readonly IRefreshTokenGenerator _refreshTokenGenerator;

    public RefreshTokenService(
        IUserSessionRepository sessionRepository,
        IAccessTokenGenerator accessTokenGenerator,
        IRefreshTokenGenerator refreshTokenGenerator)
    {
        ArgumentNullException.ThrowIfNull(sessionRepository);
        ArgumentNullException.ThrowIfNull(accessTokenGenerator);
        ArgumentNullException.ThrowIfNull(refreshTokenGenerator);

        _sessionRepository = sessionRepository;
        _accessTokenGenerator = accessTokenGenerator;
        _refreshTokenGenerator = refreshTokenGenerator;
    }

    public async Task<RefreshTokenResponse> RefreshAsync(
        RefreshTokenRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            request.RefreshToken,
            nameof(request.RefreshToken));

        var refreshTokenHash =
            _refreshTokenGenerator.Hash(
                request.RefreshToken);

        var result =
            await _sessionRepository.FindForRefreshAsync(
                refreshTokenHash,
                cancellationToken);

        if (result is null)
        {
            throw new UnauthorizedException(
                "Invalid refresh token.");
        }

        var (user, currentSession) = result.Value;
        var now = DateTimeOffset.UtcNow;

        if (currentSession.RevokedAt.HasValue)
        {
            if (currentSession.ReplacedBySessionId.HasValue)
            {
                await _sessionRepository.RevokeFamilyAsync(
                    currentSession.TokenFamilyId,
                    cancellationToken);
            }

            throw new UnauthorizedException(
                "Invalid refresh token.");
        }

        if (currentSession.ExpiresAt <= now)
        {
            throw new UnauthorizedException(
                "Invalid refresh token.");
        }

        var newRefreshToken =
            _refreshTokenGenerator.Generate();

        var newRefreshTokenHash =
            _refreshTokenGenerator.Hash(
                newRefreshToken);

        var replacementSession =
            UserSession.Create(
                user.Id,
                newRefreshTokenHash,
                currentSession.TokenFamilyId,
                now.AddDays(RefreshTokenLifetimeDays));

        var rotated =
            await _sessionRepository.TryRotateAsync(
                currentSession,
                replacementSession,
                cancellationToken);

        if (!rotated)
        {
            throw new UnauthorizedException(
                "Invalid refresh token.");
        }

        var accessToken =
            _accessTokenGenerator.Generate(user, replacementSession.Id);

        return new RefreshTokenResponse(
            accessToken.Token,
            newRefreshToken,
            accessToken.ExpiresIn,
            replacementSession.ExpiresAt);
    }
}