using ApplicationRequest =
    HealthcareStore.Application.Auth.Password.RequestPasswordResetRequest;
using HealthcareStore.Application.Auth.Password;
using HealthcareStore.Api.Common.RateLimiting;

namespace HealthcareStore.Api.Auth.Password;

public static class RequestPasswordResetEndpoint
{
    public static IEndpointRouteBuilder
        MapRequestPasswordResetEndpoint(
            this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(
            "/api/v1/auth/password/reset/request",
            async (
                RequestPasswordResetApiRequest request,
                RequestPasswordResetService service,
                CancellationToken cancellationToken) =>
            {
                await service.RequestAsync(
                    new ApplicationRequest(
                        request.Email),
                    cancellationToken);

                return Results.NoContent();
            })
            .RequireRateLimiting(RateLimitPolicies.PasswordReset);

        return endpoints;
    }
}