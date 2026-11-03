using HealthcareStore.Domain.Inventory;
using HealthcareStore.Application.Helpers;

namespace HealthcareStore.Application.Abstractions.Persistence;

public interface IInventoryBatchRepository
{
    Task<InventoryBatchInsertResult> TryCreateAsync(
        InventoryBatch batch,
        CancellationToken cancellationToken);

    Task<InventoryBatch?> GetForUpdateAsync(
        Guid batchId,
        CancellationToken cancellationToken);

    Task SaveChangesAsync(
        CancellationToken cancellationToken);

    Task<InventoryBatchReceiptInfo?> GetForReceiptAsync(
        Guid batchId,
        CancellationToken cancellationToken);

    Task<PagedResult<InventoryBatchReadModel>> GetPageForReadAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Task<PagedResult<InventoryBatchReadModel>>GetPageForVariantReadAsync(
        string sku,
        int page,
        int pageSize,
        CancellationToken cancellationToken);
}

public enum InventoryBatchInsertResult
{
    Created = 1,
    DuplicateBatchNumber = 2
}