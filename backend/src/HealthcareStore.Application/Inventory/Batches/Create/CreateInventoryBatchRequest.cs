namespace HealthcareStore.Application.Inventory.Batches.Create;

public sealed record CreateInventoryBatchRequest(
    Guid ProductVariantId,
    string BatchNumber,
    DateOnly? ExpiryDate,
    DateOnly? SellableUntil,
    decimal Mrp);