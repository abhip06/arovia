using System.Security.Claims;
using ApplicationChangePasswordRequest =
    HealthcareStore.Application.Auth.Password.ChangePasswordRequest;
using HealthcareStore.Application.Auth.Password;
using HealthcareStore.Api.Common.RateLimiting;

namespace HealthcareStore.Api.Auth.Password;

public static class ChangePasswordEndpoint
{
    public static IEndpointRouteBuilder MapChangePasswordEndpoint(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(
            "/api/v1/auth/password/change",
            async (
                ClaimsPrincipal user,
                ChangePasswordApiRequest request,
                ChangePasswordService service,
                CancellationToken cancellationToken) =>
            {
                var userIdValue =
                    user.FindFirstValue(
                        ClaimTypes.NameIdentifier);

                if (!Guid.TryParse(
                        userIdValue,
                        out var userId))
                {
                    return Results.Unauthorized();
                }

                await service.ChangeAsync(
                    new ApplicationChangePasswordRequest(
                        userId,
                        request.CurrentPassword,
                        request.NewPassword),
                    cancellationToken);

                return Results.NoContent();
            })
            .RequireAuthorization()
            .RequireRateLimiting(RateLimitPolicies.Authentication);

        return endpoints;
    }
}