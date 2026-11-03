namespace HealthcareStore.Application.Products.Get;

public sealed record ProductCatalogAvailabilityRow(
    Guid ProductVariantId,
    decimal Mrp);