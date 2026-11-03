using HealthcareStore.Domain.Inventory;

namespace HealthcareStore.Application.Helpers;

public sealed record InventoryBatchReceiptInfo(
    InventoryBatchStatus Status,
    DateOnly? ExpiryDate,
    DateOnly? SellableUntil);