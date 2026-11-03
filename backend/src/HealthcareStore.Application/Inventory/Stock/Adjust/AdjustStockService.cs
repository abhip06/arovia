using HealthcareStore.Application.Abstractions.Authentication;
using HealthcareStore.Application.Abstractions.Caching;
using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Application.Common.Caching;
using HealthcareStore.Application.Common.Exceptions;
using HealthcareStore.Domain.Inventory;

namespace HealthcareStore.Application.Inventory.Stock.Adjust;

public sealed class AdjustStockService(
    IInventoryBatchRepository batchRepository,
    IWarehouseRepository warehouseRepository,
    IWarehouseStockRepository warehouseStockRepository,
    ITransactionManager transactionManager,
    ICurrentUser currentUser,
    ICacheService cache)
{
    public async Task<AdjustStockResponse> AdjustAsync(
        AdjustStockRequest request,
        CancellationToken cancellationToken)
    {
        if (request.QuantityDelta == 0)
        {
            throw new ArgumentException(
                "Adjustment quantity cannot be zero.",
                nameof(request.QuantityDelta));
        }

        if (string.IsNullOrWhiteSpace(request.Reason))
        {
            throw new ArgumentException(
                "Adjustment reason is required.",
                nameof(request.Reason));
        }

        await transactionManager.BeginAsync(
            cancellationToken);

        try
        {
            var batch =
                await batchRepository.GetForReceiptAsync(
                    request.InventoryBatchId,
                    cancellationToken) ?? throw new NotFoundException(
                        "The specified inventory batch was not found.");

            var warehouseStatus =
                await warehouseRepository.GetStatusAsync(
                    request.WarehouseId,
                    cancellationToken) ?? throw new NotFoundException(
                        "The specified warehouse was not found.");

            if (warehouseStatus != WarehouseStatus.Active)
            {
                throw new ConflictException(
                    "Stock cannot be adjusted in an inactive warehouse.");
            }

            var warehouseStock =
                await warehouseStockRepository.GetForUpdateAsync(
                    request.InventoryBatchId,
                    request.WarehouseId,
                    cancellationToken) ?? throw new NotFoundException(
                        "No warehouse stock exists for the specified inventory batch.");

            try
            {
                warehouseStock.Adjust(
                    request.QuantityDelta);
            }
            catch (InvalidOperationException ex)
            {
                throw new ConflictException(ex.Message);
            }

            var movement =
                InventoryStockMovement.Create(
                    warehouseStock.Id,
                    InventoryMovementType.Adjusted,
                    request.QuantityDelta,
                    currentUser.UserId,
                    note: request.Reason);

            warehouseStockRepository.AddMovement(
                movement);

            var saveResult =
                await warehouseStockRepository.TrySaveChangesAsync(
                    cancellationToken);

            if (saveResult != WarehouseStockSaveResult.Saved)
            {
                throw new ConflictException(
                    "The warehouse stock could not be adjusted.");
            }

            await transactionManager.CommitAsync(
                cancellationToken);

            await cache.RemoveByTagAsync(
                CacheTags.WarehouseStocksForWarehouse(warehouseStock.WarehouseId),
                cancellationToken);
            
            await cache.RemoveByTagAsync(
                CacheTags.InventoryMovementHistory(warehouseStock.Id),
                cancellationToken);

            return new AdjustStockResponse(
                warehouseStock.Id,
                warehouseStock.InventoryBatchId,
                warehouseStock.WarehouseId,
                request.QuantityDelta,
                warehouseStock.QuantityOnHand,
                warehouseStock.QuantityReserved,
                warehouseStock.AvailableQuantity,
                movement.Id);
        }
        catch
        {
            await transactionManager.RollbackAsync(
                cancellationToken);

            throw;
        }
    }
}