using HealthcareStore.Application.Users.Update;
using System.Security.Claims;

namespace HealthcareStore.Api.Users.UpdateCurrentUser;

public static class UpdateCurrentUserEndpoint
{
    public static IEndpointRouteBuilder MapUpdateCurrentUserEndpoint(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPut(
            "/api/v1/users/me",
            async (
                ClaimsPrincipal user,
                UpdateCurrentUserRequest request,
                UpdateCurrentUserService service,
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

                var result =
                    await service.UpdateAsync(
                        new UpdateCurrentUserRequest(
                            userId,
                            request.FirstName,
                            request.LastName,
                            request.Phone,
                            request.Gender,
                            request.DateOfBirth),
                        cancellationToken);

                return Results.Ok(result);
            })
            .RequireAuthorization();

        return endpoints;
    }
}