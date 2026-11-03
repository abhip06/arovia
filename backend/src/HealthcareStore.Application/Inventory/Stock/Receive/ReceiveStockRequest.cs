namespace HealthcareStore.Application.Inventory.Stock.Receive;

public sealed record ReceiveStockRequest(
    Guid InventoryBatchId,
    Guid WarehouseId,
    int Quantity,
    decimal CostPrice);