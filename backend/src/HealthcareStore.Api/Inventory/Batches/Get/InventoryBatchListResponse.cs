namespace HealthcareStore.Api.Inventory.Batches.Get;

public sealed record InventoryBatchListResponse(
    IReadOnlyList<InventoryBatchResponse> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages);

public sealed record InventoryBatchResponse(
    string Sku,
    string BatchNumber,
    DateOnly? ExpiryDate,
    DateOnly? SellableUntil,
    decimal Mrp,
    string Status);