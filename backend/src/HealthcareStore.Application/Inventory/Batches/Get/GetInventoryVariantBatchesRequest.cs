namespace HealthcareStore.Application.Inventory.Batches.Get;

public sealed record GetInventoryVariantBatchesRequest(
    string Sku,
    int Page);