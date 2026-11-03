using HealthcareStore.Application.Abstractions.Caching;
using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Application.Common.Caching;
using HealthcareStore.Application.Helpers;
using HealthcareStore.Application.Orders.Models;
using HealthcareStore.Domain.Orders;
using Microsoft.Extensions.Logging;

namespace HealthcareStore.Application.Orders;

public sealed class ChangeOrderStatusService(
    IOrderRepository orderRepository,
    IOrderInventoryReservationService inventoryReservationService,
    IOrderPrescriptionVerificationReader prescriptionVerificationReader,
    ITransactionManager transactionManager,
    ICacheService cache,
    TimeProvider timeProvider,
    ILogger<ChangeOrderStatusService> logger)
{
    public async Task<OrderMutationResult> ChangeAsync(
        Guid orderId,
        OrderStatus targetStatus,
        Guid adminUserId,
        CancellationToken cancellationToken = default)
    {
        if (orderId == Guid.Empty)
        {
            throw new ArgumentException(
                "Order ID is required.",
                nameof(orderId));
        }

        if (adminUserId == Guid.Empty)
        {
            throw new ArgumentException(
                "Administrator user ID is required.",
                nameof(adminUserId));
        }

        if (!Enum.IsDefined(typeof(OrderStatus), targetStatus) ||
            targetStatus is OrderStatus.Placed or OrderStatus.Cancelled)
        {
            return new OrderMutationResult(
                OrderMutationStatus.InvalidStatus);
        }

        await transactionManager.BeginAsync(cancellationToken);

        try
        {
            var order = await orderRepository.GetByIdForUpdateAsync(
                orderId,
                cancellationToken);

            if (order is null)
            {
                return await AbortAsync(OrderMutationStatus.NotFound);
            }

            if (targetStatus == OrderStatus.Processing &&
                order.Items.Any(item => item.RequiresPrescription))
            {
                var verified =
                    await prescriptionVerificationReader
                        .AreAllRequiredPrescriptionsVerifiedAsync(
                            order.Id,
                            cancellationToken);

                if (!verified)
                {
                    return await AbortAsync(
                        OrderMutationStatus.PrescriptionVerificationRequired);
                }
            }

            var now = timeProvider.GetUtcNow();

            try
            {
                switch (targetStatus)
                {
                    case OrderStatus.Confirmed:
                        order.Confirm(now);
                        break;

                    case OrderStatus.Processing:
                        order.StartProcessing(now);
                        break;

                    case OrderStatus.Shipped:
                        order.MarkShipped(now);
                        break;

                    case OrderStatus.OutForDelivery:
                        order.MarkOutForDelivery(now);
                        break;

                    case OrderStatus.Delivered:
                        order.MarkDelivered(now);
                        break;

                    default:
                        return await AbortAsync(
                            OrderMutationStatus.InvalidStatus);
                }
            }
            catch (InvalidOperationException exception)
            {
                logger.LogWarning(
                    exception,
                    "Order {OrderId} cannot transition from {CurrentStatus} to {TargetStatus}.",
                    order.Id,
                    order.Status,
                    targetStatus);

                return await AbortAsync(
                    OrderMutationStatus.InvalidTransition);
            }

            if (targetStatus == OrderStatus.Shipped)
            {
                await inventoryReservationService.FulfillForOrderAsync(
                    order,
                    adminUserId,
                    now,
                    cancellationToken);
            }

            await orderRepository.SaveChangesAsync(cancellationToken);

            var details = await orderRepository.GetByIdAsync(
                order.Id,
                cancellationToken);

            if (details is null)
            {
                throw new InvalidOperationException(
                    "The updated order could not be read back.");
            }

            await transactionManager.CommitAsync(cancellationToken);

            logger.LogInformation(
                "Order {OrderId} changed to {OrderStatus} by administrator {AdminUserId}.",
                order.Id,
                targetStatus,
                adminUserId);

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
                "Failed to roll back an order status transaction.");
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