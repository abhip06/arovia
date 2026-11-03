using System.Security.Claims;
using HealthcareStore.Application.Common.Exceptions;

namespace HealthcareStore.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetRequiredUserId(
        this ClaimsPrincipal user)
    {
        var userIdValue =
            user.FindFirstValue(
                ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(
                userIdValue,
                out var userId))
        {
            throw new UnauthorizedException(
                "The authenticated user ID is invalid.");
        }

        return userId;
    }
}