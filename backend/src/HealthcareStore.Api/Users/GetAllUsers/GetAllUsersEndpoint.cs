using HealthcareStore.Api.Common.Authorization;
using HealthcareStore.Api.Common.RateLimiting;
using HealthcareStore.Api.Extensions;
using HealthcareStore.Application.Users.GetAll;
using System.Security.Claims;

namespace HealthcareStore.Api.Users.GetAllUsers;

public static class GetAllUsersEndpoint
{
    public static IEndpointRouteBuilder MapGetAllUsersEndpoint(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(
            "/api/v1/admin/users",
            async (
            ClaimsPrincipal principal,
            GetAllUsersService getAllUsersService,
            CancellationToken cancellationToken) =>
            {
                var adminUserId = principal.GetRequiredUserId();

                var response = await getAllUsersService.GetAllUsersAsync(
                    adminUserId,
                    cancellationToken);

                return Results.Ok(response);
            })
            .RequireAuthorization(AuthorizationPolicies.AdminOnly)
            .RequireRateLimiting(RateLimitPolicies.GeneralApi);

        return endpoints;
    }
}