namespace HealthcareStore.Application.Products.Get;

public sealed record ProductCatalogItem(
    Guid ProductVariantId,
    string ProductName,
    string ProductSlug,
    string? BrandName,
    string Sku,
    decimal PackageQuantity,
    string PackageUnit,
    decimal? NetQuantity,
    string? NetQuantityUnit,
    decimal? Mrp,
    decimal SellingPrice,
    bool? IsAvailable,
    string? ImageUrl);