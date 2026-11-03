using HealthcareStore.Api.Common.Authentication;
using HealthcareStore.Application.Common.Exceptions;
using HealthcareStore.Application.Auth.RefreshToken;
using ApplicationRefreshTokenRequest =
    HealthcareStore.Application.Auth.RefreshToken.RefreshTokenRequest;

namespace HealthcareStore.Api.Auth.RefreshToken;

public static class RefreshTokenEndpoint
{
    public static IEndpointRouteBuilder MapRefreshTokenEndpoint(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(
            "/api/v1/auth/refresh",
            async (
                RefreshTokenService service,
                RefreshTokenCookie refreshTokenCookie,
                HttpRequest request,
                HttpResponse response,
                CancellationToken cancellationToken) =>
            {
                var refreshToken =
                    refreshTokenCookie.Get(request);

                if (string.IsNullOrWhiteSpace(refreshToken))
                {
                    throw new UnauthorizedException(
                        "Invalid refresh token.");
                }

                var applicationRequest =
                    new ApplicationRefreshTokenRequest(
                        refreshToken);

                var result = await service.RefreshAsync(
                    applicationRequest,
                    cancellationToken);

                refreshTokenCookie.Append(
                    response,
                    result.RefreshToken,
                    result.RefreshTokenExpiresAt);

                return Results.Ok(
                    new RefreshTokenApiResponse(
                        result.AccessToken,
                        result.ExpiresIn));
            });

        return endpoints;
    }
}