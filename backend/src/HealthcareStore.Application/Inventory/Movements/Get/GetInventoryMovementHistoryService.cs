using HealthcareStore.Application.Abstractions.Caching;
using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Application.Common.Caching;
using HealthcareStore.Application.Common.Pagination;

namespace HealthcareStore.Application.Inventory.Movements.Get;

public sealed class GetInventoryMovementHistoryService(
    IInventoryStockMovementRepository repository,
    ICacheService cache)
{
    public async Task<GetInventoryMovementHistoryResponse> GetAsync(
        GetInventoryMovementHistoryRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Page < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(request.Page),
                "Page must be greater than zero.");
        }

        const int pageSize =
            PaginationDefaults.MovementPageSize;

        var fingerprint =
            CacheKeyFingerprint.Create(request.Page);

        return await cache.GetOrCreateAsync(
            CacheKeys.InventoryMovementHistory(
                request.WarehouseStockId,
                fingerprint),
            async token =>
            {
                var items =
                    await repository.GetPageAsync(
                        request.WarehouseStockId,
                        (request.Page - 1) * pageSize,
                        pageSize + 1,
                        token);

                var hasNextPage =
                    items.Count > pageSize;

                var resultItems =
                    hasNextPage
                        ? items.Take(pageSize)
                        : items;

                return new GetInventoryMovementHistoryResponse(
                    resultItems
                        .Select(static x =>
                            new InventoryMovementItem(
                                x.Id,
                                x.WarehouseStockId,
                                x.MovementType,
                                x.QuantityDelta,
                                x.CreatedByUserId,
                                x.ReferenceType,
                                x.ReferenceId,
                                x.Note,
                                x.CreatedAt))
                        .ToList(),
                    request.Page,
                    pageSize,
                    hasNextPage);
            },
            expiration: TimeSpan.FromMinutes(1),
            localCacheExpiration: TimeSpan.FromSeconds(15),
            tags:
            [
                CacheTags.InventoryMovementHistory(
                    request.WarehouseStockId)
            ],
            cancellationToken: cancellationToken);
    }
}