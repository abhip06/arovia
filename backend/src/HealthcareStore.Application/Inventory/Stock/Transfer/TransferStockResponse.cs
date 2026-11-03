namespace HealthcareStore.Application.Inventory.Stock.Transfer;

public sealed record TransferStockResponse(
    Guid SourceWarehouseStockId,
    Guid DestinationWarehouseStockId,
    Guid InventoryBatchId,
    Guid SourceWarehouseId,
    Guid DestinationWarehouseId,
    int QuantityTransferred,
    int SourceQuantityOnHand,
    int DestinationQuantityOnHand,
    Guid TransferReferenceId,
    Guid TransferOutMovementId,
    Guid TransferInMovementId);