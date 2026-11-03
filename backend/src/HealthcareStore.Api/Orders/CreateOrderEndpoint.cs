using HealthcareStore.Api.Common.RateLimiting;
using HealthcareStore.Api.Extensions;
using HealthcareStore.Application.Orders;
using HealthcareStore.Application.Orders.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HealthcareStore.Api.Orders;

public static class CreateOrderEndpoint
{
    public static IEndpointRouteBuilder MapCreateOrderEndpoint(
        this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("/api/v1/orders")
            .RequireAuthorization()
            .RequireRateLimiting(RateLimitPolicies.Orders);

        group.MapPost(
            "",
            async Task<IResult> (
                [FromBody] CreateOrderRequest request,
                ClaimsPrincipal principal,
                [FromHeader(Name = "Idempotency-Key")]
                string? idempotencyKey,
                CreateOrderService service,
                CancellationToken cancellationToken) =>
            {
                var userId = principal.GetRequiredUserId();

                if (request.ShippingAddressId == Guid.Empty)
                {
                    return Results.ValidationProblem(
                        new Dictionary<string, string[]>
                        {
                            ["shippingAddressId"] =
                            [
                                "A shipping address is required."
                            ]
                        });
                }

                if (string.IsNullOrWhiteSpace(idempotencyKey) ||
                    idempotencyKey.Trim().Length > 128)
                {
                    return Results.ValidationProblem(
                        new Dictionary<string, string[]>
                        {
                            ["Idempotency-Key"] =
                            [
                                "Provide an Idempotency-Key containing 1 to 128 characters."
                            ]
                        });
                }

                var result = await service.CreateAsync(
                    userId,
                    request,
                    idempotencyKey,
                    cancellationToken);

                return result.Status switch
                {
                    CreateOrderStatus.Created
                        when result.Order is not null =>
                        Results.Created(
                            $"/api/v1/orders/{result.Order.Id}",
                            result),

                    CreateOrderStatus.Replayed
                        when result.Order is not null =>
                        Results.Ok(result.Order),

                    CreateOrderStatus.CartEmpty =>
                        Results.Conflict(new
                        {
                            code = "cart_empty",
                            message = "Your cart is empty."
                        }),

                    CreateOrderStatus.ShippingAddressNotFound =>
                        Results.NotFound(new
                        {
                            code = "shipping_address_not_found",
                            message =
                                "The selected shipping address was not found."
                        }),

                    CreateOrderStatus.RecipientPhoneRequired =>
                        Results.ValidationProblem(
                            new Dictionary<string, string[]>
                            {
                                ["recipientPhone"] =
                                [
                                    "Add a recipient phone number to your address or account before checkout."
                                ]
                            }),

                    CreateOrderStatus.InvalidShippingAddress =>
                        Results.ValidationProblem(
                            new Dictionary<string, string[]>
                            {
                                ["shippingAddress"] =
                                [
                                    "The selected shipping address is incomplete or invalid."
                                ]
                            }),

                    CreateOrderStatus.ItemsUnavailable =>
                        Results.Conflict(new
                        {
                            code = "items_unavailable",
                            message =
                                "One or more cart items are unavailable or their prices have changed."
                        }),

                    CreateOrderStatus.InsufficientStock =>
                        Results.Conflict(new
                        {
                            code = "insufficient_stock",
                            message =
                                "The available stock changed during checkout. Please review your cart and try again."
                        }),

                    CreateOrderStatus.IdempotencyConflict =>
                        Results.Conflict(new
                        {
                            code = "idempotency_key_reused",
                            message =
                                "This Idempotency-Key was already used for a different request."
                        }),

                    _ => Results.Problem(
                        statusCode:
                            StatusCodes.Status500InternalServerError,
                        title:
                            "Unexpected order-creation result.")
                };
            });

        return endpoints;
    }
}