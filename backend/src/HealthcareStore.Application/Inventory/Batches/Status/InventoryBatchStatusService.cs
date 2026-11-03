using HealthcareStore.Application.Abstractions.Caching;
using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Application.Common.Caching;
using HealthcareStore.Application.Common.Exceptions;
using HealthcareStore.Domain.Inventory;

namespace HealthcareStore.Application.Inventory.Batches.Status;

public sealed class InventoryBatchStatusService(
    IInventoryBatchRepository batchRepository,
    ICacheService cache)
{
    public Task QuarantineAsync(
        Guid batchId,
        CancellationToken cancellationToken)
    {
        return ChangeStatusAsync(
            batchId,
            batch => batch.Quarantine(),
            cancellationToken);
    }

    public Task RecallAsync(
        Guid batchId,
        CancellationToken cancellationToken)
    {
        return ChangeStatusAsync(
            batchId,
            batch => batch.Recall(),
            cancellationToken);
    }

    public Task ActivateAsync(
        Guid batchId,
        CancellationToken cancellationToken)
    {
        return ChangeStatusAsync(
            batchId,
            batch => batch.Activate(),
            cancellationToken);
    }

    private async Task ChangeStatusAsync(
        Guid batchId,
        Action<InventoryBatch> changeStatus,
        CancellationToken cancellationToken)
    {
        var batch =
            await batchRepository.GetForUpdateAsync(
                batchId,
                cancellationToken)
            ?? throw new NotFoundException(
                "The specified inventory batch was not found.");

        try
        {
            changeStatus(batch);
        }
        catch (InvalidOperationException ex)
        {
            throw new ConflictException(ex.Message);
        }

        await batchRepository.SaveChangesAsync(
            cancellationToken);

        await cache.RemoveByTagAsync(
            CacheTags.Batches,
            cancellationToken);
    }
}