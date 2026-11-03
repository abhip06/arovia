using HealthcareStore.Application.Abstractions.Authentication;
using HealthcareStore.Application.Abstractions.Caching;
using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Application.Common.Caching;
using HealthcareStore.Application.Common.Exceptions;
using HealthcareStore.Domain.Inventory;

namespace HealthcareStore.Application.Inventory.Stock.Transfer;

public sealed class TransferStockService(
    IInventoryBatchRepository batchRepository,
    IWarehouseRepository warehouseRepository,
    IWarehouseStockRepository warehouseStockRepository,
    ITransactionManager transactionManager,
    ICurrentUser currentUser,
    ICacheService cache)
{
    private const int MaxAttempts = 3;

    public async Task<TransferStockResponse> TransferAsync(
        TransferStockRequest request,
        CancellationToken cancellationToken)
    {
        if (request.SourceWarehouseId == request.DestinationWarehouseId)
        {
            throw new ConflictException(
                "Source and destination warehouses must be different.");
        }

        if (request.Quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(request.Quantity),
                "Transfer quantity must be greater than zero.");
        }

        for (var attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            await transactionManager.BeginAsync(
                cancellationToken);

            try
            {
                var batch =
                    await batchRepository.GetForReceiptAsync(
                        request.InventoryBatchId,
                        cancellationToken)
                    ?? throw new NotFoundException(
                        "The specified inventory batch was not found.");

                if (batch.Status != InventoryBatchStatus.Active)
                {
                    throw new ConflictException(
                        "Stock cannot be transferred for an inactive inventory batch.");
                }

                var sourceWarehouseStatus =
                    await warehouseRepository.GetStatusAsync(
                        request.SourceWarehouseId,
                        cancellationToken) ?? throw new NotFoundException(
                        "The source warehouse was not found.");

                if (sourceWarehouseStatus != WarehouseStatus.Active)
                {
                    throw new ConflictException(
                        "The source warehouse is inactive.");
                }

                var destinationWarehouseStatus =
                    await warehouseRepository.GetStatusAsync(
                        request.DestinationWarehouseId,
                        cancellationToken) ?? throw new NotFoundException(
                        "The destination warehouse was not found.");

                if (destinationWarehouseStatus !=
                    WarehouseStatus.Active)
                {
                    throw new ConflictException(
                        "The destination warehouse is inactive.");
                }

                var firstWarehouseId =
                    request.SourceWarehouseId.CompareTo(
                        request.DestinationWarehouseId) < 0
                        ? request.SourceWarehouseId
                        : request.DestinationWarehouseId;

                var secondWarehouseId =
                    request.SourceWarehouseId.CompareTo(
                        request.DestinationWarehouseId) < 0
                        ? request.DestinationWarehouseId
                        : request.SourceWarehouseId;

                WarehouseStock? firstStock =
                    await warehouseStockRepository.GetForUpdateAsync(
                        request.InventoryBatchId,
                        firstWarehouseId,
                        cancellationToken);

                WarehouseStock? secondStock =
                    await warehouseStockRepository.GetForUpdateAsync(
                        request.InventoryBatchId,
                        secondWarehouseId,
                        cancellationToken);

                var sourceStock =
                    firstWarehouseId == request.SourceWarehouseId
                        ? firstStock
                        : secondStock;

                var destinationStock =
                    firstWarehouseId == request.DestinationWarehouseId
                        ? firstStock
                        : secondStock;

                if (sourceStock is null)
                {
                    throw new NotFoundException(
                        "No source stock exists for the specified inventory batch and warehouse.");
                }

                try
                {
                    sourceStock.TransferOut(
                        request.Quantity);
                }
                catch (InvalidOperationException ex)
                {
                    throw new ConflictException(ex.Message);
                }

                if (destinationStock is null)
                {
                    destinationStock = WarehouseStock.Create(
                        request.InventoryBatchId,
                        request.DestinationWarehouseId,
                        sourceStock.CostPrice,
                        request.Quantity,
                        DateTimeOffset.UtcNow);

                    warehouseStockRepository.Add(
                        destinationStock);
                }
                else
                {
                    if (destinationStock.CostPrice !=
                        sourceStock.CostPrice)
                    {
                        throw new ConflictException(
                            "The source and destination stock have different cost prices.");
                    }

                    destinationStock.Receive(
                        request.Quantity);
                }

                var transferReferenceId = Guid.NewGuid();

                var transferOutMovement =
                    InventoryStockMovement.Create(
                        sourceStock.Id,
                        InventoryMovementType.TransferredOut,
                        -request.Quantity,
                        currentUser.UserId,
                        referenceType: "InventoryTransfer",
                        referenceId: transferReferenceId,
                        note: request.Note);

                var transferInMovement =
                    InventoryStockMovement.Create(
                        destinationStock.Id,
                        InventoryMovementType.TransferredIn,
                        request.Quantity,
                        currentUser.UserId,
                        referenceType: "InventoryTransfer",
                        referenceId: transferReferenceId,
                        note: request.Note);

                warehouseStockRepository.AddMovement(
                    transferOutMovement);

                warehouseStockRepository.AddMovement(
                    transferInMovement);

                var saveResult =
                    await warehouseStockRepository.TrySaveChangesAsync(
                        cancellationToken);

                if (saveResult ==
                    WarehouseStockSaveResult.DuplicateStock)
                {
                    await transactionManager.RollbackAsync(
                        cancellationToken);

                    continue;
                }

                await transactionManager.CommitAsync(
                    cancellationToken);

                await cache.RemoveByTagAsync(
                    CacheTags.WarehouseStocksForWarehouse(request.SourceWarehouseId),
                    cancellationToken);
                
                await cache.RemoveByTagAsync(
                    CacheTags.WarehouseStocksForWarehouse(request.DestinationWarehouseId),
                    cancellationToken);

                await cache.RemoveByTagAsync(
                    CacheTags.InventoryMovementHistory(sourceStock.Id),
                    cancellationToken);
                
                await cache.RemoveByTagAsync(
                    CacheTags.InventoryMovementHistory(destinationStock.Id),
                    cancellationToken);

                return new TransferStockResponse(
                    sourceStock.Id,
                    destinationStock.Id,
                    request.InventoryBatchId,
                    request.SourceWarehouseId,
                    request.DestinationWarehouseId,
                    request.Quantity,
                    sourceStock.QuantityOnHand,
                    destinationStock.QuantityOnHand,
                    transferReferenceId,
                    transferOutMovement.Id,
                    transferInMovement.Id);
            }
            catch
            {
                await transactionManager.RollbackAsync(
                    cancellationToken);

                throw;
            }
        }

        throw new ConflictException(
            "The stock transfer could not be completed because another operation was creating the destination stock.");
    }
}