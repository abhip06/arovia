using HealthcareStore.Application.Abstractions.Caching;
using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Application.Common.Caching;
using HealthcareStore.Application.Common.Exceptions;
using HealthcareStore.Application.Common.Pagination;
using HealthcareStore.Application.Helpers;

namespace HealthcareStore.Application.Inventory.Stock.Get;

public sealed class GetWarehouseStockService(
    IWarehouseStockRepository warehouseStockRepository,
    ICacheService cache)
{
    public async Task<GetWarehouseStockResponse> GetAsync(
        Guid inventoryBatchId,
        Guid warehouseId,
        CancellationToken cancellationToken)
    {
        var stock =
            await warehouseStockRepository.GetForReadAsync(
                inventoryBatchId,
                warehouseId,
                cancellationToken) ?? throw new NotFoundException(
                "Stock was not found for the specified batch and warehouse.");

        return new GetWarehouseStockResponse(
            stock.Id,
            stock.InventoryBatchId,
            stock.WarehouseId,
            stock.CostPrice,
            stock.QuantityOnHand,
            stock.QuantityReserved,
            stock.AvailableQuantity,
            stock.ReceivedAt);
    }

    public async Task<GetWarehouseStockPageResponse> GetPageAsync(
    Guid warehouseId,
    int page,
    CancellationToken cancellationToken)
    {
        if (warehouseId == Guid.Empty)
        {
            throw new ArgumentException(
                "Warehouse ID cannot be empty.",
                nameof(warehouseId));
        }

        if (page < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(page),
                "Page must be greater than zero.");
        }

        const int pageSize =
            PaginationDefaults.StockPageSize;

        var fingerprint =
            CacheKeyFingerprint.Create(
                warehouseId,
                page);

        return await cache.GetOrCreateAsync(
            CacheKeys.WarehouseStocks(
                warehouseId,
                fingerprint),
            async token =>
            {
                var result =
                    await warehouseStockRepository
                        .GetPageForWarehouseReadAsync(
                            warehouseId,
                            page,
                            pageSize,
                            token);

                var items =
                    result.Items
                        .Select(static stock =>
                            new WarehouseStockItem(
                                stock.Id,
                                stock.InventoryBatchId,
                                stock.WarehouseId,
                                stock.Sku,
                                stock.BatchNumber,
                                stock.ExpiryDate,
                                stock.CostPrice,
                                stock.QuantityOnHand,
                                stock.QuantityReserved,
                                stock.AvailableQuantity,
                                stock.ReceivedAt))
                        .ToList();

                return new GetWarehouseStockPageResponse(
                    new PagedResult<WarehouseStockItem>(
                        items,
                        result.Page,
                        result.PageSize,
                        result.TotalCount));
            },
            expiration: TimeSpan.FromMinutes(1),
            localCacheExpiration: TimeSpan.FromSeconds(15),
            tags:
            [
                CacheTags.WarehouseStocks,
            CacheTags.WarehouseStocksForWarehouse(warehouseId)
            ],
            cancellationToken: cancellationToken);
    }
}