using HealthcareStore.Application.Abstractions.Authentication;
using HealthcareStore.Application.Abstractions.Caching;
using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Application.Common.Caching;
using HealthcareStore.Application.Common.Exceptions;
using HealthcareStore.Domain.Inventory;

namespace HealthcareStore.Application.Inventory.Stock.Receive;

public sealed class ReceiveStockService(
    IInventoryBatchRepository batchRepository,
    IWarehouseRepository warehouseRepository,
    IWarehouseStockRepository warehouseStockRepository,
    ITransactionManager transactionManager,
    ICurrentUser currentUser,
    ICacheService cache)
{
    private const int MaxAttempts = 3;

    public async Task<ReceiveStockResponse> ReceiveAsync(
        ReceiveStockRequest request,
        CancellationToken cancellationToken)
    {
        for (var attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            await transactionManager.BeginAsync(cancellationToken);

            try
            {
                var batch =
                    await batchRepository.GetForReceiptAsync(
                        request.InventoryBatchId,
                        cancellationToken) ?? throw new NotFoundException(
                            "The specified inventory batch was not found.");

                if (batch.Status != InventoryBatchStatus.Active)
                {
                    throw new ConflictException(
                        "Stock cannot be received for an inactive inventory batch.");
                }

                var today = DateOnly.FromDateTime(
                    DateTime.UtcNow);

                if (batch.ExpiryDate.HasValue &&
                    batch.ExpiryDate.Value < today)
                {
                    throw new ConflictException(
                        "Stock cannot be received for an expired inventory batch.");
                }

                if (batch.SellableUntil.HasValue &&
                    batch.SellableUntil.Value < today)
                {
                    throw new ConflictException(
                        "Stock cannot be received after the batch sellable-until date.");
                }

                var warehouseStatus =
                    await warehouseRepository.GetStatusAsync(
                        request.WarehouseId,
                        cancellationToken) ?? throw new NotFoundException(
                            "The specified warehouse was not found.");

                if (warehouseStatus != WarehouseStatus.Active)
                {
                    throw new ConflictException(
                        "Stock cannot be received into an inactive warehouse.");
                }

                var warehouseStock =
                    await warehouseStockRepository.GetForUpdateAsync(
                        request.InventoryBatchId,
                        request.WarehouseId,
                        cancellationToken);

                if (warehouseStock is null)
                {
                    warehouseStock = WarehouseStock.Create(
                        request.InventoryBatchId,
                        request.WarehouseId,
                        request.CostPrice,
                        request.Quantity,
                        DateTimeOffset.UtcNow);

                    warehouseStockRepository.Add(
                        warehouseStock);
                }
                else
                {
                    if (warehouseStock.CostPrice !=
                        request.CostPrice)
                    {
                        throw new ConflictException(
                            "The cost price does not match the existing warehouse stock for this batch.");
                    }

                    warehouseStock.Receive(
                        request.Quantity);
                }

                var movement =
                    InventoryStockMovement.Create(
                        warehouseStock.Id,
                        InventoryMovementType.Received,
                        request.Quantity,
                        currentUser.UserId,
                        note: "Stock received into warehouse.");

                warehouseStockRepository.AddMovement(
                    movement);

                var saveResult =
                    await warehouseStockRepository.TrySaveChangesAsync(
                        cancellationToken);

                if (saveResult == WarehouseStockSaveResult.DuplicateStock)
                {
                    await transactionManager.RollbackAsync(
                        cancellationToken);

                    continue;
                }

                await transactionManager.CommitAsync(cancellationToken);

                await cache.RemoveByTagAsync(
                    CacheTags.WarehouseStocksForWarehouse(warehouseStock.WarehouseId),
                    cancellationToken);

                await cache.RemoveByTagAsync(
                    CacheTags.InventoryMovementHistory(warehouseStock.Id),
                    cancellationToken);

                return new ReceiveStockResponse(
                    warehouseStock.Id,
                    warehouseStock.InventoryBatchId,
                    warehouseStock.WarehouseId,
                    warehouseStock.CostPrice,
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

        throw new ConflictException(
            "The stock could not be received because another operation was creating the same warehouse stock.");
    }
}