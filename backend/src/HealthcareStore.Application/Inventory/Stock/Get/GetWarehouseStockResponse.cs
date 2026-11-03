namespace HealthcareStore.Application.Inventory.Stock.Get;

public sealed record GetWarehouseStockResponse(
    Guid Id,
    Guid InventoryBatchId,
    Guid WarehouseId,
    decimal CostPrice,
    int QuantityOnHand,
    int QuantityReserved,
    int AvailableQuantity,
    DateTimeOffset ReceivedAt);