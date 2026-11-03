using HealthcareStore.Application.Abstractions.Caching;
using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Application.Common.Caching;
using HealthcareStore.Application.Helpers;
using HealthcareStore.Application.Orders.Models;

namespace HealthcareStore.Application.Orders;

public sealed class GetOrdersService(
    IOrderRepository orderReadRepository,
    ICacheService cache)
{
    private static readonly TimeSpan CacheExpiration =
        TimeSpan.FromSeconds(60);

    private static readonly TimeSpan LocalCacheExpiration =
        TimeSpan.FromSeconds(15);

    public Task<PagedResult<OrderSummaryModel>> GetAdminOrdersAsync(
        OrderListQuery query,
        CancellationToken cancellationToken = default)
    {
        var normalizedQuery = query.NormalizeAndValidate();

        var fingerprint = CreateFingerprint(normalizedQuery);

        return cache.GetOrCreateAsync(
            CacheKeys.AdminOrders(fingerprint),
            token => new ValueTask<PagedResult<OrderSummaryModel>>(
                orderReadRepository.GetAdminOrdersAsync(
                    normalizedQuery,
                    token)),
            expiration: CacheExpiration,
            localCacheExpiration: LocalCacheExpiration,
            tags: [CacheTags.AdminOrders],
            cancellationToken: cancellationToken).AsTask();
    }

    public Task<PagedResult<OrderSummaryModel>> GetCustomerOrdersAsync(
        Guid userId,
        OrderListQuery query,
        CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException(
                "User ID is required.",
                nameof(userId));
        }

        var normalizedQuery = query.NormalizeAndValidate();

        var fingerprint = CreateFingerprint(normalizedQuery);

        return cache.GetOrCreateAsync(
            CacheKeys.UserOrders(userId, fingerprint),
            token => new ValueTask<PagedResult<OrderSummaryModel>>(
                orderReadRepository.GetCustomerOrdersAsync(
                    userId,
                    normalizedQuery,
                    token)),
            expiration: CacheExpiration,
            localCacheExpiration: LocalCacheExpiration,
            tags: [CacheTags.UserOrders(userId)],
            cancellationToken: cancellationToken).AsTask();
    }

    public Task<OrderDetailsModel?> GetOrderByIdAsync(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        if (orderId == Guid.Empty)
        {
            throw new ArgumentException(
                "Order ID is required.",
                nameof(orderId));
        }

        return orderReadRepository.GetByIdAsync(
            orderId,
            cancellationToken);
    }

    public Task<OrderDetailsModel?> GetCustomerOrderByIdAsync(
        Guid orderId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        if (orderId == Guid.Empty)
        {
            throw new ArgumentException(
                "Order ID is required.",
                nameof(orderId));
        }

        if (userId == Guid.Empty)
        {
            throw new ArgumentException(
                "User ID is required.",
                nameof(userId));
        }

        // Ownership is enforced by the repository query.
        return orderReadRepository.GetCustomerOrderByIdAsync(
            orderId,
            userId,
            cancellationToken);
    }

    private static string CreateFingerprint(OrderListQuery query) =>
        CacheKeyFingerprint.Create(
            query.Page,
            query.PageSize,
            query.Status,
            query.CreatedFromUtc,
            query.CreatedToUtc,
            query.OrderNumber);
}