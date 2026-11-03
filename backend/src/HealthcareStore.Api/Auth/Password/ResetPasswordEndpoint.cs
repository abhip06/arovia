using ApplicationResetPasswordRequest =
    HealthcareStore.Application.Auth.Password.ResetPasswordRequest;
using HealthcareStore.Application.Auth.Password;
using HealthcareStore.Api.Common.RateLimiting;

namespace HealthcareStore.Api.Auth.Password;

public static class ResetPasswordEndpoint
{
    public static IEndpointRouteBuilder MapResetPasswordEndpoint(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(
            "/api/v1/auth/password/reset",
            async (
                ResetPasswordApiRequest request,
                ResetPasswordService service,
                CancellationToken cancellationToken) =>
            {
                await service.ResetAsync(
                    new ApplicationResetPasswordRequest(
                        request.Token,
                        request.NewPassword),
                    cancellationToken);

                return Results.NoContent();
            })
            .RequireRateLimiting(RateLimitPolicies.PasswordReset);

        return endpoints;
    }
}