using HealthcareStore.Application.Helpers;

namespace HealthcareStore.Application.Inventory.Batches.Get;

public sealed record GetInventoryBatchesResponse(
    PagedResult<InventoryBatchItem> Result);

public sealed record InventoryBatchItem(
    string Sku,
    string BatchNumber,
    DateOnly? ExpiryDate,
    DateOnly? SellableUntil,
    decimal Mrp,
    string Status);