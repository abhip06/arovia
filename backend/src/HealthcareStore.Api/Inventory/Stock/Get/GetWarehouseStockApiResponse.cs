namespace HealthcareStore.Api.Inventory.Stock.Get;

public sealed record GetWarehouseStockApiResponse(
    Guid Id,
    Guid InventoryBatchId,
    Guid WarehouseId,
    decimal CostPrice,
    int QuantityOnHand,
    int QuantityReserved,
    int AvailableQuantity,
    DateTimeOffset ReceivedAt);