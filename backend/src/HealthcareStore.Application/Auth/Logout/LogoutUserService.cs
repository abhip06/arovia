using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Application.Abstractions.Security;

namespace HealthcareStore.Application.Auth.Logout;

public sealed class LogoutUserService(
    IUserSessionRepository userSessionRepository,
    IRefreshTokenGenerator refreshTokenGenerator)
{
    public async Task LogoutAsync(
        LogoutUserRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return;
        }

        var refreshTokenHash =
            refreshTokenGenerator.Hash(request.RefreshToken);

        await userSessionRepository.RevokeAsync(
            refreshTokenHash,
            cancellationToken);
    }
}