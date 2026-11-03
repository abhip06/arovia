using HealthcareStore.Api.Common.Authentication;
using Microsoft.AspNetCore.Authentication;

namespace HealthcareStore.Api.Auth.Google;

public static class GoogleLoginEndpoint
{
    public static IEndpointRouteBuilder MapGoogleLoginEndpoint(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(
            "/api/v1/auth/google",
            () =>
            {
                var properties =
                    new AuthenticationProperties
                    {
                        RedirectUri =
                            "/api/v1/auth/google/complete"
                    };

                return Results.Challenge(
                    properties,
                    [
                        ExternalAuthenticationDefaults.GoogleScheme
                    ]);
            });

        return endpoints;
    }
}