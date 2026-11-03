namespace HealthcareStore.Api.Catalog.Get;

public sealed record ProductCatalogResponse(
    IReadOnlyList<ProductCatalogItemResponse> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages);

public sealed record ProductCatalogItemResponse(
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
    string? ImageUrl);