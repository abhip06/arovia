using HealthcareStore.Application.Abstractions.Caching;
using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Application.Common.Caching;
using HealthcareStore.Application.Orders.Models;
using Microsoft.Extensions.Logging;

namespace HealthcareStore.Application.Orders;

public sealed class CancelOrderService(
    IOrderRepository orderRepository,
    IOrderInventoryReservationService inventoryReservationService,
    ITransactionManager transactionManager,
    ICacheService cache,
    TimeProvider timeProvider,
    ILogger<CancelOrderService> logger)
{
    public Task<OrderMutationResult> CancelForCustomerAsync(
        Guid orderId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User ID is required.", nameof(userId));
        }

        return CancelAsync(
            orderId,
            userId,
            userId,
            cancellationToken);
    }

    public Task<OrderMutationResult> CancelForAdminAsync(
        Guid orderId,
        Guid adminUserId,
        CancellationToken cancellationToken = default)
    {
        if (adminUserId == Guid.Empty)
        {
            throw new ArgumentException(
                "Administrator user ID is required.",
                nameof(adminUserId));
        }

        return CancelAsync(
            orderId,
            null,
            adminUserId,
            cancellationToken);
    }

    private async Task<OrderMutationResult> CancelAsync(
        Guid orderId,
        Guid? ownerUserId,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        if (orderId == Guid.Empty)
        {
            throw new ArgumentException(
                "Order ID is required.",
                nameof(orderId));
        }

        await transactionManager.BeginAsync(cancellationToken);

        try
        {
            var order = ownerUserId.HasValue
                ? await orderRepository.GetCustomerOrderForUpdateAsync(
                    orderId,
                    ownerUserId.Value,
                    cancellationToken)
                : await orderRepository.GetByIdForUpdateAsync(
                    orderId,
                    cancellationToken);

            if (order is null)
            {
                return await AbortAsync(OrderMutationStatus.NotFound);
            }

            // A retry must never release the same stock twice.
            if (order.Status == Domain.Orders.OrderStatus.Cancelled)
            {
                var existingDetails = await orderRepository.GetByIdAsync(
                    order.Id,
                    cancellationToken);

                await transactionManager.CommitAsync(cancellationToken);

                return new OrderMutationResult(
                    OrderMutationStatus.AlreadyCancelled,
                    existingDetails);
            }

            var now = timeProvider.GetUtcNow();

            try
            {
                order.Cancel(now);
            }
            catch (InvalidOperationException)
            {
                return await AbortAsync(
                    OrderMutationStatus.InvalidTransition);
            }

            // Changes stock reservations and allocation states.
            // It does not save independently; the repository call below
            // saves everything within this transaction.
            await inventoryReservationService.ReleaseForOrderAsync(
                order,
                now,
                cancellationToken);

            await orderRepository.SaveChangesAsync(cancellationToken);

            var details = await orderRepository.GetByIdAsync(
                order.Id,
                cancellationToken);

            if (details is null)
            {
                throw new InvalidOperationException(
                    "The cancelled order could not be read back.");
            }

            await transactionManager.CommitAsync(cancellationToken);

            logger.LogInformation(
                "Order {OrderId} cancelled by user {ActorUserId}.",
                order.Id,
                actorUserId);

            await InvalidateOrderCachesAsync(order.UserId);

            return new OrderMutationResult(
                OrderMutationStatus.Succeeded,
                details);
        }
        catch
        {
            await RollbackSafelyAsync();
            throw;
        }
    }

    private async Task<OrderMutationResult> AbortAsync(
        OrderMutationStatus status)
    {
        await transactionManager.RollbackAsync(CancellationToken.None);

        return new OrderMutationResult(status);
    }

    private async Task RollbackSafelyAsync()
    {
        try
        {
            await transactionManager.RollbackAsync(CancellationToken.None);
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Failed to roll back an order cancellation transaction.");
        }
    }

    private async Task InvalidateOrderCachesAsync(Guid userId)
    {
        try
        {
            await cache.RemoveByTagAsync(
                CacheTags.AdminOrders,
                CancellationToken.None);
        }
        catch (Exception exception)
        {
            logger.LogWarning(
                exception,
                "Failed to invalidate administrator order-list cache.");
        }

        try
        {
            await cache.RemoveByTagAsync(
                CacheTags.UserOrders(userId),
                CancellationToken.None);
        }
        catch (Exception exception)
        {
            logger.LogWarning(
                exception,
                "Failed to invalidate order-list cache for user {UserId}.",
                userId);
        }
    }
}