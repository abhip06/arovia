using HealthcareStore.Domain.Inventory;

namespace HealthcareStore.Api.Inventory.Batches.Create;

public sealed record CreateInventoryBatchApiResponse(
    Guid Id,
    Guid ProductVariantId,
    string BatchNumber,
    DateOnly? ExpiryDate,
    DateOnly? SellableUntil,
    decimal Mrp,
    InventoryBatchStatus Status);