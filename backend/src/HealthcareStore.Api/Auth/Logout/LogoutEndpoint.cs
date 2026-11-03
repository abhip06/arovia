using ApplicationLogoutRequest =
    HealthcareStore.Application.Auth.Logout.LogoutUserRequest;

using HealthcareStore.Api.Common.Authentication;
using HealthcareStore.Application.Auth.Logout;

namespace HealthcareStore.Api.Auth.Logout;

public static class LogoutEndpoint
{
    public static IEndpointRouteBuilder MapLogoutEndpoint(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(
            "/api/v1/auth/logout",
            async (
                HttpRequest request,
                HttpResponse response,
                LogoutUserService service,
                RefreshTokenCookie refreshTokenCookie,
                CancellationToken cancellationToken) =>
            {
                var refreshToken =
                    refreshTokenCookie.Get(request);

                await service.LogoutAsync(
                    new ApplicationLogoutRequest(
                        refreshToken ?? string.Empty),
                    cancellationToken);

                refreshTokenCookie.Delete(response);

                return Results.NoContent();
            });

        return endpoints;
    }
}