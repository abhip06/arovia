namespace HealthcareStore.Api.Inventory.Stock.Get;

public sealed record WarehouseStockListApiResponse(
    IReadOnlyList<WarehouseStockApiResponse> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages);

public sealed record WarehouseStockApiResponse(
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