using HealthcareStore.Api.Common.Authorization;
using HealthcareStore.Api.Common.RateLimiting;
using HealthcareStore.Api.Extensions;
using HealthcareStore.Application.Users.ChangeRole;
using System.Security.Claims;

namespace HealthcareStore.Api.Users.ChangeRole;

public static class ChangeUserRoleEndpoint
{
    public static IEndpointRouteBuilder MapChangeUserRoleEndpoint(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(
            "/api/v1/admin/users/{userId:guid}/role",
            async (
                Guid userId,
                ChangeUserRoleRequest request,
                ClaimsPrincipal principal,
                ChangeUserRoleService changeUserRoleService,
                CancellationToken cancellationToken) =>
            {
                var adminUserId = principal.GetRequiredUserId();

                await changeUserRoleService.ChangeUserRole(
                    adminUserId,
                    new ChangeUserRoleRequestDto(
                        userId,
                        request.Role),
                    cancellationToken);

                return Results.NoContent();
            })
            .RequireAuthorization(AuthorizationPolicies.AdminOnly)
            .RequireRateLimiting(RateLimitPolicies.GeneralApi);

        return endpoints;
    }
}