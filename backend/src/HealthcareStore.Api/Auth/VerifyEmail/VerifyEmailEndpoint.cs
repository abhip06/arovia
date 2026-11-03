using HealthcareStore.Api.Common.RateLimiting;
using HealthcareStore.Application.Auth.VerifyEmail;

using ApplicationVerifyEmailRequest =
    HealthcareStore.Application.Auth.VerifyEmail.VerifyEmailRequest;

namespace HealthcareStore.Api.Auth.VerifyEmail;

public static class VerifyEmailEndpoint
{
    public static IEndpointRouteBuilder MapVerifyEmailEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(
            "/api/v1/auth/email-verification/verify",
            async (
                VerifyEmailRequest request,
                VerifyEmailService service,
                CancellationToken cancellationToken) =>
            {
                var applicationRequest = new ApplicationVerifyEmailRequest(
                    request.Token);

                var response = await service.VerifyAsync(
                    applicationRequest,
                    cancellationToken);

                return Results.Ok(response);
            })
            .RequireRateLimiting(RateLimitPolicies.Authentication);

        return endpoints;
    }
}
