using HealthcareStore.Api.Common.RateLimiting;
using HealthcareStore.Api.Extensions;
using HealthcareStore.Application.Abstractions.Location;
using HealthcareStore.Application.Checkout;
using System.Security.Claims;

namespace HealthcareStore.Api.Checkout;

public static class CheckoutEndpoints
{
    public static IEndpointRouteBuilder MapCheckoutEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("/api/v1/me/checkout")
            .RequireAuthorization();

        group
            .MapGet("", GetCheckoutAsync)
            .RequireRateLimiting(
                RateLimitPolicies.CartRead);

        return endpoints;
    }

    private static async Task<IResult> GetCheckoutAsync(
        ClaimsPrincipal user,
        HttpContext httpContext,
        string? pincode,
        IPincodeContextResolver pincodeContextResolver,
        CheckoutService checkoutService,
        CancellationToken cancellationToken)
    {
        var userId = user.GetRequiredUserId();

        // Reject an invalid supplied pincode rather than
        // silently falling back to IP-based resolution.
        /*if (!string.IsNullOrWhiteSpace(pincode) &&
            !IsValidPincode(pincode))
        {
            return Results.BadRequest(new
            {
                message = "A valid 6-digit pincode is required."
            });
        }*/

        var pincodeContext =
            await pincodeContextResolver.ResolveAsync(
                pincode,
                httpContext.Connection.RemoteIpAddress,
                cancellationToken);

        // A pincode is required to calculate the checkout quote.
        if (pincodeContext is null)
        {
            return Results.BadRequest(new
            {
                message =
                    "Could not determine your delivery pincode. Please provide a valid 6-digit pincode."
            });
        }

        var checkout = await checkoutService.GetAsync(
            userId,
            pincodeContext.Pincode,
            cancellationToken);

        return Results.Ok(checkout);
    }

    private static bool IsValidPincode(string? pincode)
    {
        if (string.IsNullOrWhiteSpace(pincode))
        {
            return false;
        }

        var value = pincode.Trim();

        return value.Length == 6 &&
               value.All(
                   static character =>
                       character is >= '0' and <= '9');
    }
}