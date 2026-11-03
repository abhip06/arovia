namespace HealthcareStore.Api.Inventory.Stock.Adjust;

public sealed record AdjustStockApiResponse(
    Guid WarehouseStockId,
    Guid InventoryBatchId,
    Guid WarehouseId,
    int QuantityDelta,
    int QuantityOnHand,
    int QuantityReserved,
    int AvailableQuantity,
    Guid MovementId);