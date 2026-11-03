namespace HealthcareStore.Application.Helpers;

public sealed record WarehouseStockListReadModel(
    Guid Id,
    Guid InventoryBatchId,
    Guid WarehouseId,
    string Sku,
    string BatchNumber,
    DateOnly? ExpiryDate,
    decimal CostPrice,
    int QuantityOnHand,
    int QuantityReserved,
    int AvailableQuantity,
    DateTimeOffset ReceivedAt);