namespace HealthcareStore.Api.Inventory.Stock.Receive;

public sealed record ReceiveStockApiResponse(
    Guid WarehouseStockId,
    Guid InventoryBatchId,
    Guid WarehouseId,
    decimal CostPrice,
    int QuantityOnHand,
    int QuantityReserved,
    int AvailableQuantity,
    Guid MovementId);