namespace HealthcareStore.Application.Inventory.Stock.Adjust;

public sealed record AdjustStockResponse(
    Guid WarehouseStockId,
    Guid InventoryBatchId,
    Guid WarehouseId,
    int QuantityDelta,
    int QuantityOnHand,
    int QuantityReserved,
    int AvailableQuantity,
    Guid MovementId);