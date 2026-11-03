using HealthcareStore.Api.Common.Authorization;
using HealthcareStore.Api.Common.RateLimiting;
using HealthcareStore.Api.Extensions;
using HealthcareStore.Application.Orders;
using HealthcareStore.Application.Orders.Models;
using System.Security.Claims;

namespace HealthcareStore.Api.Endpoints.Orders;

public static class OrderMutationEndpoints
{
    public static IEndpointRouteBuilder MapOrderMutationEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var customerOrders = endpoints
            .MapGroup("/api/v1/orders")
            .RequireAuthorization()
            .RequireRateLimiting(RateLimitPolicies.Orders);

        customerOrders.MapPost(
            "/{orderId:guid}/cancel",
            async Task<IResult> (
                Guid orderId,
                ClaimsPrincipal principal,
                CancelOrderService service,
                CancellationToken cancellationToken) =>
            {
                var userId = principal.GetRequiredUserId();

                var result = await service.CancelForCustomerAsync(
                    orderId,
                    userId,
                    cancellationToken);

                return ToHttpResult(result);
            });

        var adminOrders = endpoints
            .MapGroup("/api/v1/admin/orders")
            .RequireAuthorization(AuthorizationPolicies.AdminOnly)
            .RequireRateLimiting(RateLimitPolicies.Orders);

        adminOrders.MapPost(
            "/{orderId:guid}/cancel",
            async Task<IResult> (
                Guid orderId,
                ClaimsPrincipal principal,
                CancelOrderService service,
                CancellationToken cancellationToken) =>
            {
                var adminUserId = principal.GetRequiredUserId();

                var result = await service.CancelForAdminAsync(
                    orderId,
                    adminUserId,
                    cancellationToken);

                return ToHttpResult(result);
            });

        adminOrders.MapPost(
            "/{orderId:guid}/status",
            async Task<IResult> (
                Guid orderId,
                ChangeOrderStatusRequest request,
                ClaimsPrincipal principal,
                ChangeOrderStatusService service,
                CancellationToken cancellationToken) =>
            {
                var adminUserId = principal.GetRequiredUserId();

                var result = await service.ChangeAsync(
                    orderId,
                    request.Status,
                    adminUserId,
                    cancellationToken);

                return ToHttpResult(result);
            });

        return endpoints;
    }

    private static IResult ToHttpResult(OrderMutationResult result)
    {
        return result.Status switch
        {
            OrderMutationStatus.Succeeded =>
                Results.Ok(result.Order),

            OrderMutationStatus.AlreadyCancelled =>
                Results.Ok(result.Order),

            OrderMutationStatus.NotFound =>
                Results.NotFound(),

            OrderMutationStatus.InvalidTransition =>
                Results.Conflict(new
                {
                    code = "invalid_order_transition",
                    message =
                        "The requested operation is not allowed for the current order status."
                }),

            OrderMutationStatus.InvalidStatus =>
                Results.ValidationProblem(
                    new Dictionary<string, string[]>
                    {
                        ["status"] =
                        [
                            "The requested order status is invalid for this endpoint."
                        ]
                    }),

            OrderMutationStatus.PrescriptionVerificationRequired =>
                Results.Conflict(new
                {
                    code = "prescription_verification_required",
                    message =
                        "Required prescriptions must be verified before the order can enter processing."
                }),

            _ => Results.Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Unexpected order mutation result.")
        };
    }
}