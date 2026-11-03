using HealthcareStore.Application.Abstractions.Caching;
using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Application.Common.Caching;
using HealthcareStore.Application.Common.Pagination;
using HealthcareStore.Application.Helpers;

namespace HealthcareStore.Application.Inventory.Batches.Get;

public sealed class GetInventoryBatchesService(
    IInventoryBatchRepository inventoryBatchRepository,
    ICacheService cache)
{
    public async Task<GetInventoryBatchesResponse> GetAllAsync(
        GetInventoryBatchesRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Page < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(request.Page),
                "Page must be greater than zero.");
        }

        const int pageSize =
            PaginationDefaults.BatchPageSize;

        var fingerprint =
            CacheKeyFingerprint.Create(request.Page);

        return await cache.GetOrCreateAsync(
            CacheKeys.InventoryBatches(fingerprint),
            async token =>
            {
                var result =
                    await inventoryBatchRepository.GetPageForReadAsync(
                        request.Page,
                        pageSize,
                        token);

                var items =
                    result.Items
                        .Select(static batch =>
                            new InventoryBatchItem(
                                batch.Sku,
                                batch.BatchNumber,
                                batch.ExpiryDate,
                                batch.SellableUntil,
                                batch.Mrp,
                                batch.Status))
                        .ToList();

                return new GetInventoryBatchesResponse(
                    new PagedResult<InventoryBatchItem>(
                        items,
                        result.Page,
                        result.PageSize,
                        result.TotalCount));
            },
            expiration: TimeSpan.FromMinutes(1),
            localCacheExpiration: TimeSpan.FromSeconds(15),
            tags:
            [
                CacheTags.Batches
            ],
            cancellationToken: cancellationToken);
    }

    public async Task<GetInventoryBatchesResponse> GetByVariantAsync(
        GetInventoryVariantBatchesRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Sku);

        if (request.Page < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(request.Page),
                "Page must be greater than zero.");
        }

        var normalizedSku =
            request.Sku.Trim().ToUpperInvariant();

        const int pageSize =
            PaginationDefaults.BatchPageSize;

        var fingerprint =
            CacheKeyFingerprint.Create(
                normalizedSku,
                request.Page);

        return await cache.GetOrCreateAsync(
            CacheKeys.InventoryBatches(
                normalizedSku,
                fingerprint),
            async token =>
            {
                var result =
                    await inventoryBatchRepository
                        .GetPageForVariantReadAsync(
                            normalizedSku,
                            request.Page,
                            pageSize,
                            token);

                var items =
                    result.Items
                        .Select(batch =>
                            new InventoryBatchItem(
                                normalizedSku,
                                batch.BatchNumber,
                                batch.ExpiryDate,
                                batch.SellableUntil,
                                batch.Mrp,
                                batch.Status))
                        .ToList();

                return new GetInventoryBatchesResponse(
                    new PagedResult<InventoryBatchItem>(
                        items,
                        result.Page,
                        result.PageSize,
                        result.TotalCount));
            },
            expiration: TimeSpan.FromMinutes(1),
            localCacheExpiration: TimeSpan.FromSeconds(15),
            tags:
            [
                CacheTags.Batches
            ],
            cancellationToken: cancellationToken);
    }
}