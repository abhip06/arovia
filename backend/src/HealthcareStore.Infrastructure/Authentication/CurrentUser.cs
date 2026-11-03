using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using HealthcareStore.Application.Abstractions.Authentication;

namespace HealthcareStore.Infrastructure.Authentication;

public sealed class CurrentUser(
    IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public Guid UserId
    {
        get
        {
            var userId =
                httpContextAccessor.HttpContext?
                    .User
                    .FindFirstValue(ClaimTypes.NameIdentifier)
                ?? httpContextAccessor.HttpContext?
                    .User
                    .FindFirstValue("sub");

            if (!Guid.TryParse(userId, out var id))
            {
                throw new InvalidOperationException(
                    "Authenticated user ID is missing or invalid.");
            }

            return id;
        }
    }
}