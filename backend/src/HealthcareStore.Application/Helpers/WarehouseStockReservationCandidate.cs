namespace HealthcareStore.Application.Helpers;

public sealed record WarehouseStockReservationCandidate(
    Guid WarehouseStockId,
    Guid InventoryBatchId,
    Guid WarehouseId,
    Guid ProductVariantId,
    int AvailableQuantity,
    DateOnly? ExpiryDate,
    DateOnly? SellableUntil,
    DateTimeOffset ReceivedAt,
    DateTimeOffset BatchCreatedAt);