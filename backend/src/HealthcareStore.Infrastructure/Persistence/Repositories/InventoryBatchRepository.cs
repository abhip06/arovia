using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Application.Helpers;
using HealthcareStore.Domain.Inventory;
using HealthcareStore.Infrastructure.Persistence.Extensions;
using HealthcareStore.Infrastructure.Persistence.Helpers;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace HealthcareStore.Infrastructure.Persistence.Repositories;

public sealed class InventoryBatchRepository(
    ApplicationDbContext _dbContext) : IInventoryBatchRepository
{
    private const string BatchNumberIndexName = "IX_inventory_batches_ProductVariantId_BatchNumber";

    public async Task<InventoryBatchInsertResult> TryCreateAsync(
        InventoryBatch batch,
        CancellationToken cancellationToken)
    {
        _dbContext.InventoryBatches.Add(batch);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);

            return InventoryBatchInsertResult.Created;
        }
        catch (DbUpdateException ex)
            when (PostgresExceptionHelper.IsUniqueViolation(
                ex,
                BatchNumberIndexName))
        {
            _dbContext.Detach(batch);

            return InventoryBatchInsertResult.DuplicateBatchNumber;
        }
    }

    public Task<InventoryBatch?> GetForUpdateAsync(
        Guid batchId,
        CancellationToken cancellationToken)
    {
        return _dbContext.InventoryBatches
            .FirstOrDefaultAsync(
                x => x.Id == batchId,
                cancellationToken);
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(
            cancellationToken);
    }

    public Task<InventoryBatchReceiptInfo?> GetForReceiptAsync(
        Guid batchId,
        CancellationToken cancellationToken)
    {
        return _dbContext.InventoryBatches
            .AsNoTracking()
            .Where(x => x.Id == batchId)
            .Select(x => new InventoryBatchReceiptInfo(
                x.Status,
                x.ExpiryDate,
                x.SellableUntil))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PagedResult<InventoryBatchReadModel>>GetPageForReadAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var query =
            from batch in _dbContext.InventoryBatches.AsNoTracking()
            join variant in _dbContext.ProductVariants.AsNoTracking()
                on batch.ProductVariantId equals variant.Id
            select new
            {
                Batch = batch,
                variant.Sku
            };

        var totalCount =
            await query.CountAsync(cancellationToken);

        var items =
            await query
                .OrderBy(x => x.Batch.ProductVariantId)
                .ThenBy(x => x.Batch.BatchNumber)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new InventoryBatchReadModel(
                    x.Sku,
                    x.Batch.BatchNumber,
                    x.Batch.ExpiryDate,
                    x.Batch.SellableUntil,
                    x.Batch.Mrp,
                    x.Batch.Status.ToString()))
                .ToListAsync(cancellationToken);

        return new PagedResult<InventoryBatchReadModel>(
            items,
            page,
            pageSize,
            totalCount);
    }

    public async Task<PagedResult<InventoryBatchReadModel>>GetPageForVariantReadAsync(
        string sku,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var normalizedSku =
    sku.Trim().ToUpperInvariant();

        var query =
            from batch in _dbContext.InventoryBatches.AsNoTracking()
            join variant in _dbContext.ProductVariants.AsNoTracking()
                on batch.ProductVariantId equals variant.Id
            where variant.Sku == normalizedSku
            select new
            {
                batch,
                variant.Sku
            };

        var totalCount =
            await query.CountAsync(cancellationToken);

        var items =
            await query
                .OrderBy(x => x.batch.BatchNumber)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new InventoryBatchReadModel(
                    x.Sku,
                    x.batch.BatchNumber,
                    x.batch.ExpiryDate,
                    x.batch.SellableUntil,
                    x.batch.Mrp,
                    x.batch.Status.ToString()))
                .ToListAsync(cancellationToken);

        return new PagedResult<InventoryBatchReadModel>(
            items,
            page,
            pageSize,
            totalCount);
    }
}