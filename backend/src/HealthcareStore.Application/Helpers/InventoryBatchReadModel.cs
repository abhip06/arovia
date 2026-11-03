namespace HealthcareStore.Application.Helpers;

public sealed record InventoryBatchReadModel(
    string Sku,
    string BatchNumber,
    DateOnly? ExpiryDate,
    DateOnly? SellableUntil,
    decimal Mrp,
    string Status);