using HealthcareStore.Application.Helpers;

namespace HealthcareStore.Application.Inventory.Stock.Get;

public sealed record GetWarehouseStockPageResponse(
    PagedResult<WarehouseStockItem> Result);

public sealed record WarehouseStockItem(
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