using ApplicationResendEmailVerificationRequest =
    HealthcareStore.Application.Auth.ResendEmailVerification.ResendEmailVerificationRequest;
using HealthcareStore.Application.Auth.ResendEmailVerification;
using HealthcareStore.Api.Common.RateLimiting;

namespace HealthcareStore.Api.Auth.ResendEmailVerification;

public static class ResendEmailVerificationEndpoint
{
    public static IEndpointRouteBuilder MapResendEmailVerificationEndpoint(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(
            "/api/v1/auth/email-verification/resend",
            async (
                ResendEmailVerificationRequest request,
                ResendEmailVerificationService service,
                CancellationToken cancellationToken) =>
            {
                var applicationRequest =
                    new ApplicationResendEmailVerificationRequest(
                        request.Email);

                await service.ResendAsync(
                    applicationRequest,
                    cancellationToken);

                return Results.Ok(new
                {
                    message =
                        "If the email address requires verification, a verification email has been sent."
                });
            })
            .RequireRateLimiting(RateLimitPolicies.Authentication);

        return endpoints;
    }
}