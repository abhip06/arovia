using System.Security.Claims;
using HealthcareStore.Api.Extensions;
using HealthcareStore.Application.Users.GetCurrentUser;

namespace HealthcareStore.Api.Users.GetCurrentUser;

public static class GetCurrentUserEndpoint
{
    public static IEndpointRouteBuilder MapGetCurrentUserEndpoint(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(
            "/api/v1/users/me",
            async (
                ClaimsPrincipal user,
                GetCurrentUserService service,
                CancellationToken cancellationToken) =>
            {
                var userId = user.GetRequiredUserId();

                var result =
                    await service.GetAsync(
                        userId,
                        cancellationToken);

                return Results.Ok(result);
            })
            .RequireAuthorization();

        return endpoints;
    }
}