using HealthcareStore.Api.Common.Authorization;
using HealthcareStore.Api.Common.RateLimiting;
using HealthcareStore.Api.Extensions;
using HealthcareStore.Application.Orders;
using System.Security.Claims;

namespace HealthcareStore.Api.Orders;

public static class OrderQueryEndpoints
{
    public static IEndpointRouteBuilder MapOrderQueryEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var adminOrders = endpoints
            .MapGroup("/api/v1/admin/orders")
            .RequireAuthorization(AuthorizationPolicies.AdminOnly)
            .RequireRateLimiting(RateLimitPolicies.Orders);

        adminOrders.MapGet(
            "",
            async Task<IResult> (
                [AsParameters] OrderListQueryParameters query,
                GetOrdersService service,
                CancellationToken cancellationToken) =>
            {
                if (!query.TryNormalize(
                        out var normalized,
                        out var error))
                {
                    return InvalidQuery(error);
                }

                var result = await service.GetAdminOrdersAsync(
                    normalized,
                    cancellationToken);

                return Results.Ok(result);
            });

        adminOrders.MapGet(
            "/{orderId:guid}",
            async Task<IResult> (
                Guid orderId,
                GetOrdersService service,
                CancellationToken cancellationToken) =>
            {
                var order = await service.GetOrderByIdAsync(
                    orderId,
                    cancellationToken);

                return order is null
                    ? Results.NotFound()
                    : Results.Ok(order);
            });

        var customerOrders = endpoints
            .MapGroup("/api/v1/orders")
            .RequireAuthorization()
            .RequireRateLimiting(RateLimitPolicies.Orders);

        customerOrders.MapGet(
            "",
            async Task<IResult> (
                ClaimsPrincipal user,
                [AsParameters] OrderListQueryParameters query,
                GetOrdersService service,
                CancellationToken cancellationToken) =>
            {
                var userId = user.GetRequiredUserId();

                if (!query.TryNormalize(
                        out var normalized,
                        out var error))
                {
                    return InvalidQuery(error);
                }

                var result = await service.GetCustomerOrdersAsync(
                    userId,
                    normalized,
                    cancellationToken);

                return Results.Ok(result);
            });

        customerOrders.MapGet(
            "/{orderId:guid}",
            async Task<IResult> (
                Guid orderId,
                ClaimsPrincipal user,
                GetOrdersService service,
                CancellationToken cancellationToken) =>
            {
                var userId = user.GetRequiredUserId();

                var order = await service.GetCustomerOrderByIdAsync(
                    orderId,
                    userId,
                    cancellationToken);

                return order is null
                    ? Results.NotFound()
                    : Results.Ok(order);
            });

        return endpoints;
    }

    private static IResult InvalidQuery(string? error) =>
        Results.ValidationProblem(
            new Dictionary<string, string[]>
            {
                ["query"] = [error ?? "Invalid query."]
            });    
}