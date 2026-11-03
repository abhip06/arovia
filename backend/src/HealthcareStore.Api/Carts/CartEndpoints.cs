using System.Security.Claims;
using HealthcareStore.Api.Common.RateLimiting;
using HealthcareStore.Application.Carts;
using HealthcareStore.Application.Carts.Models;
using HealthcareStore.Application.Carts.Requests;
using HealthcareStore.Domain.Carts;

namespace HealthcareStore.Api.Carts;

public static class CartEndpoints
{
    public static IEndpointRouteBuilder MapCartEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var group =
            endpoints
                .MapGroup("/api/v1/me/cart")
                .RequireAuthorization();

        group
            .MapGet(
                "",
                GetCartAsync)
            .RequireRateLimiting(
                RateLimitPolicies.CartRead);

        group
            .MapPost(
                "/items",
                AddItemAsync)
            .RequireRateLimiting(
                RateLimitPolicies.CartWrite);

        group
            .MapPut(
                "/items/{cartItemId:guid}",
                SetQuantityAsync)
            .RequireRateLimiting(
                RateLimitPolicies.CartWrite);

        group
            .MapDelete(
                "/items/{cartItemId:guid}",
                RemoveItemAsync)
            .RequireRateLimiting(
                RateLimitPolicies.CartWrite);

        return endpoints;
    }

    private static async Task<IResult> GetCartAsync(
        HttpContext httpContext,
        CartService cartService,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(
                httpContext,
                out var userId))
        {
            return Results.Unauthorized();
        }

        var cart =
            await cartService.GetAsync(
                userId,
                cancellationToken);

        return Results.Ok(cart);
    }

    private static async Task<IResult> AddItemAsync(
        HttpContext httpContext,
        AddCartItemRequest request,
        CartService cartService,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(
                httpContext,
                out var userId))
        {
            return Results.Unauthorized();
        }

        if (request.ProductVariantId ==
            Guid.Empty)
        {
            return Results.BadRequest(
                new
                {
                    message =
                        "Product variant ID is required."
                });
        }

        if (request.Quantity is < 1 or > CartItem.MaxQuantity)
        {
            return Results.BadRequest(
                new
                {
                    message =
                        $"Quantity must be between 1 and {CartItem.MaxQuantity}."
                });
        }

        var idempotencyKey =
            GetIdempotencyKey(
                httpContext);

        if (string.IsNullOrWhiteSpace(
                idempotencyKey))
        {
            return Results.BadRequest(
                new
                {
                    message =
                        "Idempotency-Key header is required."
                });
        }

        if (idempotencyKey.Length > 128)
        {
            return Results.BadRequest(
                new
                {
                    message =
                        "Idempotency-Key cannot exceed 128 characters."
                });
        }

        var result =
            await cartService.AddItemAsync(
                userId,
                request,
                idempotencyKey,
                cancellationToken);

        return ToResult(result);
    }

    private static async Task<IResult> SetQuantityAsync(
        HttpContext httpContext,
        Guid cartItemId,
        SetCartItemQuantityRequest request,
        CartService cartService,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(
                httpContext,
                out var userId))
        {
            return Results.Unauthorized();
        }

        if (cartItemId == Guid.Empty)
        {
            return Results.BadRequest(
                new
                {
                    message =
                        "Cart item ID is required."
                });
        }

        if (request.Quantity is < 1 or > CartItem.MaxQuantity)
        {
            return Results.BadRequest(
                new
                {
                    message =
                        $"Quantity must be between 1 and {CartItem.MaxQuantity}."
                });
        }

        var idempotencyKey =
            GetOptionalIdempotencyKey(
                httpContext);

        if (idempotencyKey is not null &&
            idempotencyKey.Length > 128)
        {
            return Results.BadRequest(
                new
                {
                    message =
                        "Idempotency-Key cannot exceed 128 characters."
                });
        }

        var result =
            await cartService.SetItemQuantityAsync(
                userId,
                cartItemId,
                request,
                idempotencyKey,
                cancellationToken);

        return ToResult(result);
    }

    private static async Task<IResult> RemoveItemAsync(
        HttpContext httpContext,
        Guid cartItemId,
        CartService cartService,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(
                httpContext,
                out var userId))
        {
            return Results.Unauthorized();
        }

        if (cartItemId == Guid.Empty)
        {
            return Results.BadRequest(
                new
                {
                    message =
                        "Cart item ID is required."
                });
        }

        var idempotencyKey =
            GetOptionalIdempotencyKey(
                httpContext);

        if (idempotencyKey is not null &&
            idempotencyKey.Length > 128)
        {
            return Results.BadRequest(
                new
                {
                    message =
                        "Idempotency-Key cannot exceed 128 characters."
                });
        }

        var result =
            await cartService.RemoveItemAsync(
                userId,
                cartItemId,
                idempotencyKey,
                cancellationToken);

        return ToResult(result);
    }

    private static IResult ToResult(
        CartMutationResponse result)
    {
        return result.Status switch
        {
            CartMutationStatus.Applied =>
                Results.Ok(result.Cart),

            CartMutationStatus.Replayed =>
                Results.Ok(result.Cart),

            CartMutationStatus.NotFound =>
                Results.NotFound(
                    new
                    {
                        message =
                            "The requested cart resource was not found."
                    }),

            CartMutationStatus.IdempotencyConflict =>
                Results.Conflict(
                    new
                    {
                        message =
                            "The Idempotency-Key was already used for a different request."
                    }),

            CartMutationStatus.LimitExceeded =>
                Results.BadRequest(
                    new
                    {
                        message =
                            $"A cart cannot contain more than {Cart.MaxDistinctItems} distinct items."
                    }),

            _ =>
                Results.Problem(
                    "Unknown cart mutation result.")
        };
    }

    private static bool TryGetUserId(
        HttpContext httpContext,
        out Guid userId)
    {
        var value =
            httpContext.User.FindFirstValue(
                ClaimTypes.NameIdentifier)
            ??
            httpContext.User.FindFirstValue("sub");

        return Guid.TryParse(
            value,
            out userId) &&
            userId != Guid.Empty;
    }

    private static string? GetOptionalIdempotencyKey(
        HttpContext httpContext)
    {
        var value =
            httpContext.Request.Headers[
                "Idempotency-Key"]
                .FirstOrDefault();

        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }

    private static string? GetIdempotencyKey(
        HttpContext httpContext)
    {
        return GetOptionalIdempotencyKey(
            httpContext);
    }
}