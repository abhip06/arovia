using HealthcareStore.Domain.Inventory;

namespace HealthcareStore.Application.Inventory.Batches.Create;

public sealed record CreateInventoryBatchResponse(
    Guid Id,
    Guid ProductVariantId,
    string BatchNumber,
    DateOnly? ExpiryDate,
    DateOnly? SellableUntil,
    decimal Mrp,
    InventoryBatchStatus Status);