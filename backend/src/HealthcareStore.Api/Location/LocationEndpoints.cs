using HealthcareStore.Api.Location;
using HealthcareStore.Application.Abstractions.Location;
using HealthcareStore.Application.Location;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HealthcareStore.Api.Location;

public static class LocationEndpoints
{
    public static IEndpointRouteBuilder MapLocationEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints
            .MapGet("/api/v1/location/current", GetCurrentAsync)
            .AllowAnonymous()
            .RequireRateLimiting("GeneralApi")
            .WithName("GetCurrentLocation")
            .WithTags("Location")
            .Produces<CurrentLocationResponse>(
                StatusCodes.Status200OK)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized);

        return endpoints;
    }

    private static async Task<IResult> GetCurrentAsync(
        HttpContext httpContext,
        [FromServices] IPincodeContextResolver contextResolver,
        CancellationToken cancellationToken)
    {
        Guid? userId = null;
        var principal = httpContext.User;

        if (principal.Identity?.IsAuthenticated == true)
        {
            var userIdValue =
                principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? principal.FindFirst("sub")?.Value;

            if (!Guid.TryParse(userIdValue, out var authenticatedUserId) ||
                authenticatedUserId == Guid.Empty)
            {
                return Results.Unauthorized();
            }

            userId = authenticatedUserId;
        }

        var location = await contextResolver.ResolveCurrentAsync(
            userId,
            httpContext.Connection.RemoteIpAddress,
            cancellationToken);

        if (location is null)
        {
            return Results.NoContent();
        }

        var response = new CurrentLocationResponse(
            location.Pincode,
            location.City,
            location.State,
            location.Country,
            location.Source == PincodeSource.User ? "user" : "ip",
            location.IsConfirmed);

        return Results.Ok(response);
    }
}