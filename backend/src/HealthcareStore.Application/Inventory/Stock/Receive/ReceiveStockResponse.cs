namespace HealthcareStore.Application.Inventory.Stock.Receive;

public sealed record ReceiveStockResponse(
    Guid WarehouseStockId,
    Guid InventoryBatchId,
    Guid WarehouseId,
    decimal CostPrice,
    int QuantityOnHand,
    int QuantityReserved,
    int AvailableQuantity,
    Guid MovementId);