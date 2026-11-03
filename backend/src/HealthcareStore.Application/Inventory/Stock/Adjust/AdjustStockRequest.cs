namespace HealthcareStore.Application.Inventory.Stock.Adjust;

public sealed record AdjustStockRequest(
    Guid InventoryBatchId,
    Guid WarehouseId,
    int QuantityDelta,
    string Reason);