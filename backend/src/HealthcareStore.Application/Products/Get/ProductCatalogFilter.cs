namespace HealthcareStore.Application.Products.Get;

public sealed record ProductCatalogFilter(
    string? ProductType,
    string? BrandSlug,
    string? ManufacturerSlug,
    string? CategorySlug,
    string? Search);