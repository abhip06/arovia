using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Application.Helpers;
using HealthcareStore.Application.Products.Get;
using HealthcareStore.Application.Products.Get.Detail;
using HealthcareStore.Domain.Inventory;
using HealthcareStore.Infrastructure.Persistence.Helpers;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace HealthcareStore.Infrastructure.Persistence.Repositories;

public sealed class WarehouseStockRepository(
    ApplicationDbContext _dbContext) : IWarehouseStockRepository
{
    private const string StockIdentityIndexName = "IX_warehouse_stocks_InventoryBatchId_WarehouseId";

    public Task<WarehouseStock?> GetForUpdateAsync(
        Guid inventoryBatchId,
        Guid warehouseId,
        CancellationToken cancellationToken)
    {
        return _dbContext.WarehouseStocks
            .FromSqlInterpolated($"""
            SELECT *
            FROM warehouse_stocks
            WHERE "InventoryBatchId" = {inventoryBatchId}
              AND "WarehouseId" = {warehouseId}
            FOR UPDATE
            """)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public void Add(
        WarehouseStock warehouseStock)
    {
        _dbContext.WarehouseStocks.Add(warehouseStock);
    }

    public void AddMovement(
        InventoryStockMovement movement)
    {
        _dbContext.InventoryStockMovements.Add(movement);
    }

    public async Task<WarehouseStockSaveResult> TrySaveChangesAsync(
    CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.SaveChangesAsync(
                cancellationToken);

            return WarehouseStockSaveResult.Saved;
        }
        catch (DbUpdateException ex)
            when (PostgresExceptionHelper.IsUniqueViolation(
                ex,
                StockIdentityIndexName))
        {
            return WarehouseStockSaveResult.DuplicateStock;
        }
    }

    public Task<WarehouseStock?> GetForReadAsync(
        Guid inventoryBatchId,
        Guid warehouseId,
        CancellationToken cancellationToken)
    {
        return _dbContext.WarehouseStocks
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x =>
                    x.InventoryBatchId == inventoryBatchId &&
                    x.WarehouseId == warehouseId,
                cancellationToken);
    }

    public async Task<PagedResult<WarehouseStockListReadModel>>
    GetPageForWarehouseReadAsync(
        Guid warehouseId,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var query =
            from stock in _dbContext.WarehouseStocks.AsNoTracking()
            join batch in _dbContext.InventoryBatches.AsNoTracking()
                on stock.InventoryBatchId equals batch.Id
            join variant in _dbContext.ProductVariants.AsNoTracking()
                on batch.ProductVariantId equals variant.Id
            where stock.WarehouseId == warehouseId
            select new
            {
                stock,
                variant.Sku,
                batch.BatchNumber,
                batch.ExpiryDate
            };

        var totalCount =
            await query.CountAsync(cancellationToken);

        var items =
            await query
                .OrderBy(x => x.Sku)
                .ThenBy(x => x.BatchNumber)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new WarehouseStockListReadModel(
                    x.stock.Id,
                    x.stock.InventoryBatchId,
                    x.stock.WarehouseId,
                    x.Sku,
                    x.BatchNumber,
                    x.ExpiryDate,
                    x.stock.CostPrice,
                    x.stock.QuantityOnHand,
                    x.stock.QuantityReserved,
                    x.stock.QuantityOnHand -
                        x.stock.QuantityReserved,
                    x.stock.ReceivedAt))
                .ToListAsync(cancellationToken);

        return new PagedResult<WarehouseStockListReadModel>(
            items,
            page,
            pageSize,
            totalCount);
    }

    public async Task<IReadOnlyList<Guid>> GetAvailableVariantIdsAsync(
        IReadOnlyCollection<Guid> productVariantIds,
        IReadOnlyCollection<Guid> warehouseIds,
        DateOnly today,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(productVariantIds);
        ArgumentNullException.ThrowIfNull(warehouseIds);

        if (productVariantIds.Count == 0 ||
            warehouseIds.Count == 0)
        {
            return [];
        }

        var query =
            from stock in _dbContext.WarehouseStocks.AsNoTracking()
            join batch in _dbContext.InventoryBatches.AsNoTracking()
                on stock.InventoryBatchId equals batch.Id
            join warehouse in _dbContext.Warehouses.AsNoTracking()
                on stock.WarehouseId equals warehouse.Id
            where
                productVariantIds.Contains(batch.ProductVariantId) &&
                warehouseIds.Contains(stock.WarehouseId) &&
                warehouse.Status == WarehouseStatus.Active &&
                batch.Status == InventoryBatchStatus.Active &&
                stock.QuantityOnHand > stock.QuantityReserved &&
                (batch.ExpiryDate == null ||
                 batch.ExpiryDate >= today) &&
                (batch.SellableUntil == null ||
                 batch.SellableUntil >= today)
            select batch.ProductVariantId;

        return await query
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ProductCatalogAvailabilityRow>>GetAvailableCatalogVariantsAsync(
        IReadOnlyCollection<Guid> productVariantIds,
        IReadOnlyCollection<Guid> warehouseIds,
        DateOnly today,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(productVariantIds);
        ArgumentNullException.ThrowIfNull(warehouseIds);

        if (productVariantIds.Count == 0 ||
            warehouseIds.Count == 0)
        {
            return [];
        }

        var query =
            from stock in _dbContext.WarehouseStocks.AsNoTracking()
            join batch in _dbContext.InventoryBatches.AsNoTracking()
                on stock.InventoryBatchId equals batch.Id
            join warehouse in _dbContext.Warehouses.AsNoTracking()
                on stock.WarehouseId equals warehouse.Id
            where
                productVariantIds.Contains(batch.ProductVariantId) &&
                warehouseIds.Contains(stock.WarehouseId) &&
                warehouse.Status == WarehouseStatus.Active &&
                batch.Status == InventoryBatchStatus.Active &&
                stock.QuantityOnHand > stock.QuantityReserved &&
                (batch.ExpiryDate == null ||
                 batch.ExpiryDate >= today) &&
                (batch.SellableUntil == null ||
                 batch.SellableUntil >= today)
            select new
            {
                batch.ProductVariantId,
                batch.Mrp,
                batch.ExpiryDate,
                batch.SellableUntil,
                stock.ReceivedAt,
                batch.CreatedAt,
                stock.WarehouseId,
                stock.Id
            };

        return await query
            .GroupBy(x => x.ProductVariantId)
            .Select(group =>
                group
                    .OrderBy(x => x.ExpiryDate == null)
                    .ThenBy(x => x.ExpiryDate)
                    .ThenBy(x => x.SellableUntil == null)
                    .ThenBy(x => x.SellableUntil)
                    .ThenBy(x => x.ReceivedAt)
                    .ThenBy(x => x.CreatedAt)
                    .ThenBy(x => x.WarehouseId)
                    .ThenBy(x => x.Id)
                    .Select(x =>
                        new ProductCatalogAvailabilityRow(
                            x.ProductVariantId,
                            x.Mrp))
                    .First())
            .ToListAsync(cancellationToken);
    }

    public async Task<ProductDetailStockAvailabilityReadModel?>
        GetAvailableProductVariantDetailAsync(
            Guid productVariantId,
            IReadOnlyCollection<Guid> warehouseIds,
            DateOnly today,
            CancellationToken cancellationToken)
    {
        if (productVariantId == Guid.Empty)
        {
            throw new ArgumentException(
                "Product variant ID cannot be empty.",
                nameof(productVariantId));
        }

        ArgumentNullException.ThrowIfNull(warehouseIds);

        if (warehouseIds.Count == 0)
        {
            return null;
        }

        var eligibleStock =
            await (
                from stock in _dbContext.WarehouseStocks.AsNoTracking()
                join batch in _dbContext.InventoryBatches.AsNoTracking()
                    on stock.InventoryBatchId equals batch.Id
                join warehouse in _dbContext.Warehouses.AsNoTracking()
                    on stock.WarehouseId equals warehouse.Id
                where
                    batch.ProductVariantId == productVariantId &&
                    warehouseIds.Contains(stock.WarehouseId) &&
                    warehouse.Status == WarehouseStatus.Active &&
                    batch.Status == InventoryBatchStatus.Active &&
                    stock.QuantityOnHand > stock.QuantityReserved &&
                    (batch.ExpiryDate == null || batch.ExpiryDate >= today) &&
                    (batch.SellableUntil == null || batch.SellableUntil >= today)
                select new
                {
                    stock.WarehouseId,
                    AvailableQuantity =
                        stock.QuantityOnHand - stock.QuantityReserved,
                    batch.Mrp,
                    batch.ExpiryDate,
                    batch.SellableUntil,
                    stock.ReceivedAt,
                    BatchCreatedAt = batch.CreatedAt,
                    StockId = stock.Id
                })
                .ToListAsync(cancellationToken);

        var selectedWarehouseAvailability =
            eligibleStock
                .GroupBy(static row => row.WarehouseId)
                .Select(group => new
                {
                    WarehouseId = group.Key,
                    AvailableQuantity = group.Sum(
                        static row => row.AvailableQuantity),
                    Mrp = group
                        .OrderBy(static row => row.ExpiryDate == null)
                        .ThenBy(static row => row.ExpiryDate)
                        .ThenBy(static row => row.SellableUntil == null)
                        .ThenBy(static row => row.SellableUntil)
                        .ThenBy(static row => row.ReceivedAt)
                        .ThenBy(static row => row.BatchCreatedAt)
                        .ThenBy(static row => row.StockId)
                        .Select(static row => row.Mrp)
                        .First()
                })
                .OrderByDescending(static row => row.AvailableQuantity)
                .ThenBy(static row => row.WarehouseId)
                .FirstOrDefault();

        return selectedWarehouseAvailability is null
            ? null
            : new ProductDetailStockAvailabilityReadModel(
                selectedWarehouseAvailability.AvailableQuantity,
                selectedWarehouseAvailability.Mrp);
    }

    public async Task<IReadOnlyList<WarehouseStockReservationCandidate>>
    GetReservationCandidatesAsync(
        IReadOnlyCollection<Guid> productVariantIds,
        string pincode,
        DateOnly today,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(productVariantIds);
        ArgumentException.ThrowIfNullOrWhiteSpace(pincode);

        var ids = productVariantIds
            .Where(static id => id != Guid.Empty)
            .Distinct()
            .ToArray();

        if (ids.Length == 0)
        {
            return [];
        }

        var normalizedPincode = pincode.Trim();

        return await (
            from stock in _dbContext.WarehouseStocks.AsNoTracking()

            join batch in _dbContext.InventoryBatches.AsNoTracking()
                on stock.InventoryBatchId equals batch.Id

            join warehouse in _dbContext.Warehouses.AsNoTracking()
                on stock.WarehouseId equals warehouse.Id

            where
                ids.Contains(batch.ProductVariantId) &&
                warehouse.Status == WarehouseStatus.Active &&
                batch.Status == InventoryBatchStatus.Active &&
                stock.QuantityOnHand > stock.QuantityReserved &&
                (batch.ExpiryDate == null || batch.ExpiryDate >= today) &&
                (batch.SellableUntil == null || batch.SellableUntil >= today) &&
                _dbContext.WarehouseServiceablePincodes
                    .AsNoTracking()
                    .Any(serviceable =>
                        serviceable.WarehouseId == stock.WarehouseId &&
                        serviceable.Pincode == normalizedPincode)

            select new WarehouseStockReservationCandidate(
                stock.Id,
                batch.Id,
                stock.WarehouseId,
                batch.ProductVariantId,
                stock.QuantityOnHand - stock.QuantityReserved,
                batch.ExpiryDate,
                batch.SellableUntil,
                stock.ReceivedAt,
                batch.CreatedAt))
            .ToListAsync(cancellationToken);
    }

    public Task<WarehouseStock?> GetForUpdateByIdAsync(
        Guid warehouseStockId,
        CancellationToken cancellationToken)
    {
        if (warehouseStockId == Guid.Empty)
        {
            throw new ArgumentException(
                "Warehouse stock ID cannot be empty.",
                nameof(warehouseStockId));
        }

        return _dbContext.WarehouseStocks
            .FromSqlInterpolated($"""
            SELECT *
            FROM warehouse_stocks
            WHERE "Id" = {warehouseStockId}
            FOR UPDATE
            """)
            .SingleOrDefaultAsync(cancellationToken);
    }
}
